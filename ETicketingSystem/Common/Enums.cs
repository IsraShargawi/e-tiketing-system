namespace ETicketingSystem.Common;

public enum TicketType
{
    Gold = 1,
    Premium = 2,
    VIP = 3
}

public enum PaymentMethod
{
    CreditCard = 1,
    QRScan = 2
}

public enum PaymentStatus
{
    Pending = 1,
    Completed = 2,
    Failed = 3
}

public enum OrderStatus
{
    Pending = 1,
    Completed = 2,
    Cancelled = 3
}

public enum AccountCategory
{
    Asset = 1,
    Liability = 2,
    Equity = 3,
    Revenue = 4,
    Expense = 5
}
