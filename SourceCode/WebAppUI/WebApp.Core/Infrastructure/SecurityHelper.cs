using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace WebApp.Core.Infrastructure;

public class SecurityHelper
{
    private readonly IConfiguration _config;

    public SecurityHelper(IConfiguration config)
    {
        this._config = config;
    }

    public string GenerateHash(string payload = "Default Payload")
    {
        using (var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(_config["Hash:HashKey"])))
        {
            byte[] data = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            return Convert.ToBase64String(data);
        }
    }
}