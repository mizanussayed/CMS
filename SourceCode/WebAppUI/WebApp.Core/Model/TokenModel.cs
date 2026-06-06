namespace WebApp.Core.Model;

public class TokenModel
{
    public string JwtToken { get; set; }
    public DateTime Expires { get; set; }
    public string RefreshToken { get; set; }
    public DateTime RefreshTokenExpires { get; set; }
}