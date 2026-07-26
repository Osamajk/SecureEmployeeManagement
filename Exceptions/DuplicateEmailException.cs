namespace SecureEmployeeManagement.Exceptions;

// Custom exception = a domain event we can map to a specific HTTP status,
// rather than letting infrastructure exceptions leak upward.
public class DuplicateEmailException : Exception
{
    public DuplicateEmailException(string email)
        : base($"An employee with email '{email}' already exists.") { }
}