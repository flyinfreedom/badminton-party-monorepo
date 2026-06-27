using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BadmintonParty.Liff.Web.Api.Models;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;

namespace BadmintonParty.Liff.Web.Api.Services;

public class JwtService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtService> _logger;
    private readonly string _secret;
    private readonly string _issuer;
    private readonly string _audience;

    public JwtService(IConfiguration configuration, ILogger<JwtService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        
        // 預設 JWT 金鑰，至少要 32 個字元（256-bit）。生產環境應由環境變數 JWT_SECRET 覆寫。
        _secret = _configuration["Jwt:Secret"] ?? "BadmintonParty_Super_Secret_Key_For_Jwt_Security_2026";
        _issuer = _configuration["Jwt:Issuer"] ?? "BadmintonPartyApi";
        _audience = _configuration["Jwt:Audience"] ?? "BadmintonPartyLiff";
    }

    public string GenerateToken(MemberProfile profile, TimeSpan expiration)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_secret);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, profile.MemberId),
            new Claim("LineUserId", profile.LineUserId),
            new Claim("DisplayName", profile.DisplayName),
            new Claim("PictureUrl", profile.PictureUrl)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.Add(expiration),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        if (string.IsNullOrEmpty(token)) return null;

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_secret);

        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return new ClaimsPrincipal(new ClaimsIdentity(((JwtSecurityToken)validatedToken).Claims));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ValidateToken exception occurred.");
            return null;
        }
    }
}

