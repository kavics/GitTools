namespace Kavics.GittLib;

/// <summary>
/// Represents an exception that is thrown when the passed name of organization or user is not found in a Github operation.
/// </summary>
public class MissingOrganizationException : Exception
{
    public MissingOrganizationException() { }
    public MissingOrganizationException(string message) : base(message) { }
    public MissingOrganizationException(string message, Exception inner) : base(message, inner) { }
}