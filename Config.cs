namespace DbProvisioner;

public record ConfigUser(string Username, string Password, string[] Databases);

public class Config
{
    public readonly ConfigUser[] Users;
    public readonly string DbHost;
    public readonly string DbUser;
    public readonly string DbPass;
    public readonly string DbName;

    public Config()
    {
        var envVars = Environment.GetEnvironmentVariables();
        var listUsers = new List<ConfigUser>();
        foreach (var envVar in envVars.Keys)
        {
            string key = envVar?.ToString() ?? "";
            if (key.StartsWith("DB_") && key.EndsWith("_USERNAME") && key.Length > 12)
            {
                var db = key.TrimStart("DB_").TrimEnd("_USERNAME");
                Console.WriteLine($"🧑‍💻 Found db {db}");
                var username = Environment.GetEnvironmentVariable($"DB_{db}_USERNAME");
                var password = Environment.GetEnvironmentVariable($"DB_{db}_PASSWORD");
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    Console.WriteLine($"❌ Missing username or password for db {db}, skipping...");
                    continue;
                }

                listUsers.Add(new(username, password, [db.ToString()]));
            }
        }

        Users = listUsers.ToArray();

        DbHost = Environment.GetEnvironmentVariable("DB_HOST")!;
        DbUser = Environment.GetEnvironmentVariable("DB_USER")!;
        DbPass = Environment.GetEnvironmentVariable("DB_PASSWORD")!;
        DbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "mysql";
    }

    public string GetDbConnString(string? database = null)
    {
        if (database is null)
            database = DbName;

        return $"Server={DbHost};User ID={DbUser};Password={DbPass};Database={database};Port=3306;Pooling=false;";
    }
}