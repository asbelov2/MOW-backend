using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace API
{
  public class AuthOptions
  {
    public const string ISSUER = "MOW-Server"; // издатель токена
    public const string AUDIENCE = "MOW-Client"; // потребитель токена
    private const string KEY = "bd9e30600b19aecfd86491ff7228bd70";   // ключ для шифрации
    public const int LIFETIME = 60; // время жизни токена - час

    public static SymmetricSecurityKey GetSymmetricSecurityKey()
    {
      return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
    }
  }
}