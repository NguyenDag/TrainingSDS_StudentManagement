namespace StudentManagement.Domain.Exceptions;

/// <summary>Thrown when an operation would break a domain invariant.</summary>
public class DomainException(string message) : Exception(message);
