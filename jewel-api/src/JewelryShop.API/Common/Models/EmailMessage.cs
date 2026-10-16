namespace JewelryShop.Common.Models;

public record EmailMessage(string To, string Subject, string HtmlBody, string TextBody);
