namespace ChordHub.Api.Core.Exceptions;

public class ValidationException(string message, List<string> errors) : DomainException(message)
{
    public List<string> ValidationErrors { get; } = errors;

    public ValidationException(List<string> errors) : this("Validation Failed", errors)
    {
    }
}
