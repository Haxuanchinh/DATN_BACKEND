namespace SportManager.Infrastructure.Extensions;

public static class PostgresExtension
{
    [DbFunction("unaccent", "public")]
    public static string Unaccent(string input)
    {
        throw new NotImplementedException("This method can only be used with LINQ.");
    }
}
