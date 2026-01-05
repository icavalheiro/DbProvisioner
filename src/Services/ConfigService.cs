namespace DbProvisioner.Services;

public class ConfigService
{
    public record User(string Username, string Password, string[] Databases);

    public User[] Users { get; }
    public string DbHost { get; }
    public string DbUser { get; }
    public string DbPass { get; }
    public string DbName { get; }

    public ConfigService(IEnvironmentService env)
    {
        var envVars = env.GetEnvironmentVariables();
        var listUsers = new List<User>();
        const string dbEnvVarStartSegment = "DB_";
        const string dbEnvVarEndSegment = "_USERNAME";
        var dbEnvVarStartSegmentLength = dbEnvVarStartSegment.Length;
        var dbEnvVarEndSegmentLength = dbEnvVarEndSegment.Length;

        foreach (var keyObj in envVars.Keys)
        {
            string key = keyObj?.ToString() ?? "";
            var dbNameLength = key.Length - dbEnvVarStartSegmentLength - dbEnvVarEndSegmentLength;
            if (key.StartsWith(dbEnvVarStartSegment) && key.EndsWith(dbEnvVarEndSegment) && dbNameLength > 0)
            {
                var db = key.Substring(dbEnvVarStartSegmentLength, dbNameLength);
                var username = env.GetEnvironmentVariable($"DB_{db}_USERNAME");
                var password = env.GetEnvironmentVariable($"DB_{db}_PASSWORD");

                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                {
                    listUsers.Add(new(username, password, [db]));
                }
            }
        }

        Users = listUsers.ToArray();
        DbHost = env.GetEnvironmentVariable("DB_HOST") ?? "localhost";
        DbUser = env.GetEnvironmentVariable("DB_USER") ?? "root";
        DbPass = env.GetEnvironmentVariable("DB_PASSWORD") ?? "";
        DbName = env.GetEnvironmentVariable("DB_NAME") ?? "mysql";
    }

    public string GetDbConnString(string? database = null) =>
        $"Server={DbHost};User ID={DbUser};Password={DbPass};Database={database ?? DbName};Port=3306;Pooling=false;";
}