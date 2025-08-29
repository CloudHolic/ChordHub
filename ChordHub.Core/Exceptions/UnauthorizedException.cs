namespace ChordHub.Core.Exceptions;

public class UnauthorizedException(string message) : DomainException(message);