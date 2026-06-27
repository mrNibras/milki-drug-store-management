namespace MilkiDrugStore.Domain.Enums;

public enum RoleType
{
    Admin = 1,
    Pharmacist = 2
}

public enum TransactionType
{
    Purchase = 1,
    Sale = 2,
    Damage = 3,
    Expired = 4,
    Adjustment = 5
}

public enum NotificationType
{
    LowStock = 1,
    OutOfStock = 2,
    ExpiryAlert = 3,
    SystemAlert = 4
}
