using BadmintonParty.Infrastructure.Entities;
using BadmintonParty.Infrastructure.Repositories;
using BadmintonParty.Infrastructure.Enums;
using BadmintonParty.Infrastructure.Contexts;
using BadmintonParty.Infrastructure.Extensions;
using BadmintonParty.Infrastructure.Models;
namespace BadmintonParty.Liff.Web.Api.Exceptions;

public class CustomException : Exception
{
    private readonly int _statusCode;
    public CustomException(string message, int statusCode = 700) : base(message)
    {
        _statusCode = statusCode;
    }

    public int StatusCode => _statusCode;
}

