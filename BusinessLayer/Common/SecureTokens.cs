using System.Security.Cryptography;
using System.Text;

namespace BusinessLayer.Common;

// One-time tokens sent by email. Only their hash is stored, so a database leak does not expose usable links.
public static class SecureTokens
{
    public static string Generate() => Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

    public static string Hash(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
