namespace JewelryShop.Features.Orders.Common;

public static class OrderMessages
{
    public const string ProductInOrder = "Ce produit figure dans une commande.";
    public const string AddressRequired = "Veuillez renseigner votre adresse de livraison.";
    public const string RecipientNameRequired = "Le prénom est requis.";
    public const string RecipientSurnameRequired = "Le nom est requis.";
    public const string RecipientNameTooLong = "100 caractères maximum.";
    public const string CommentTooLong = "1000 caractères maximum.";
    public const string NotFound = "Commande introuvable.";
    public const string CannotBeCancelled = "Cette commande ne peut plus être annulée.";
    public const string CannotBeReceived = "Cette commande ne peut pas être marquée comme reçue.";
    public const string InvalidStatus = "Le statut n'est pas valide.";
    public const string NoteRequired = "La note est requise.";
    public const string NoteTooLong = "2000 caractères maximum.";
    public const string NoteNotFound = "Note introuvable.";
}
