namespace JewelryShop.Features.Orders.Common;

/// <summary>Qui demande le changement de statut : les transitions autorisées en dépendent.</summary>
public enum OrderActor
{
    Customer,
    Admin
}
