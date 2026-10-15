using System.Collections.Concurrent;
using System.Reflection;
using JewelryShop.Application.Common.Interfaces;
using JewelryShop.Application.Common.Models;
using Scriban;
using Scriban.Runtime;
using Scriban.Syntax;

namespace JewelryShop.Infrastructure.Email;

/// <summary>
/// Rend les templates Scriban embarqués dans l'assembly (voir <see cref="EmailTemplates"/>).
/// Chaque template est parsé une seule fois puis gardé en cache : le service est un singleton.
/// </summary>
public class ScribanEmailTemplateRenderer : IEmailTemplateRenderer
{
    private const string ResourcePrefix = "EmailTemplates.";
    private const string SubjectSuffix = ".subject.sbn";
    private const string HtmlSuffix = ".html.sbn";
    private const string TextSuffix = ".txt.sbn";

    private static readonly string[] _lineBreaks = ["\r\n", "\n", "\r"];

    private readonly Func<string, string?> _readSource;
    private readonly ConcurrentDictionary<string, ParsedEmailTemplate> _cache = new();

    public ScribanEmailTemplateRenderer()
        : this(ReadEmbeddedResource)
    {
    }

    /// <param name="readSource">Renvoie le contenu d'un fichier de template (par exemple « Test.html.sbn »), ou null s'il n'existe pas.</param>
    internal ScribanEmailTemplateRenderer(Func<string, string?> readSource)
    {
        _readSource = readSource;
    }

    public RenderedEmail Render(string templateName, object model)
    {
        var template = _cache.GetOrAdd(templateName, Parse);
        var subject = RenderPart(templateName, template.Subject, model);

        return new RenderedEmail(
            ToSingleLine(subject),
            RenderPart(templateName, template.Html, model),
            RenderPart(templateName, template.Text, model));
    }

    private ParsedEmailTemplate Parse(string templateName) =>
        new(
            ParsePart(templateName, SubjectSuffix),
            ParsePart(templateName, HtmlSuffix),
            ParsePart(templateName, TextSuffix));

    private Template ParsePart(string templateName, string suffix)
    {
        var fileName = templateName + suffix;
        var source = _readSource(fileName)
            ?? throw new InvalidOperationException(
                $"Template de mail « {templateName} » introuvable : le fichier {fileName} n'existe pas.");

        var template = Template.Parse(source, fileName);
        if (template.HasErrors)
        {
            throw new InvalidOperationException(
                $"Template de mail « {templateName} » invalide ({fileName}) : {string.Join(" ; ", template.Messages)}");
        }

        return template;
    }

    private static string RenderPart(string templateName, Template template, object model)
    {
        var scriptObject = new ScriptObject();
        scriptObject.Import(model);

        // StrictVariables : une variable absente du modèle lève une erreur au lieu de laisser un trou dans le mail.
        var context = new TemplateContext { StrictVariables = true };
        context.PushGlobal(scriptObject);

        try
        {
            return template.Render(context);
        }
        catch (ScriptRuntimeException ex)
        {
            throw new InvalidOperationException(
                $"Rendu du template de mail « {templateName} » impossible : {ex.Message}", ex);
        }
    }

    // Le sujet devient un en-tête SMTP, qui ne doit pas contenir de saut de ligne.
    private static string ToSingleLine(string subject) =>
        string.Join(' ', subject.Split(_lineBreaks, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

    private static string? ReadEmbeddedResource(string fileName)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourcePrefix + fileName);
        if (stream is null)
        {
            return null;
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private sealed record ParsedEmailTemplate(Template Subject, Template Html, Template Text);
}
