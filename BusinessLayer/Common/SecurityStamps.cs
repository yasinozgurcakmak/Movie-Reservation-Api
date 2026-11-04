namespace BusinessLayer.Common;

public static class SecurityStamps
{
    public static string New() => Guid.NewGuid().ToString("N");

    public static string CacheKey(int userId) => $"users:{userId}:security-stamp";
}
