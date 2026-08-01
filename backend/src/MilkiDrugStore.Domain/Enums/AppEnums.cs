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

public enum BatchSelectionMode
{
    AutomaticFefo = 1,
    ManualSelection = 2
}

public static class NotificationTypeStrings
{
    public const string LowStock = "LOW_STOCK";
    public const string OutOfStock = "OUT_OF_STOCK";
    public const string ExpiryAlert = "EXPIRY_ALERT";
    public const string SystemAlert = "SYSTEM_ALERT";
}
