namespace BuildLedger.Core.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

public class InsufficientStockException : DomainException
{
    public InsufficientStockException(string productName, decimal available, decimal requested)
        : base($"Insufficient stock for '{productName}'. Available: {available}, requested: {requested}.") { }
}

public class DuplicateCustomerException : DomainException
{
    public DuplicateCustomerException(string phone)
        : base($"A customer with phone number '{phone}' already exists.") { }
}
