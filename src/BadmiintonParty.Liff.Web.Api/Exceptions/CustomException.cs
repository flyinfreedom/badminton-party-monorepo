namespace BadmiintonParty.Liff.Web.Api.Exceptions;

public class CustomException : Exception
{
    private readonly int _statusCode;
    public CustomException(string message, int statusCode = 700) : base(message)
    {
        _statusCode = statusCode;
    }

    public int StatusCode => _statusCode;
}
