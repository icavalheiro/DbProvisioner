
using DbProvisioner;
using MySqlConnector;

Console.WriteLine("🔄 Loading config file and env vars...");
var config = new Config();
Console.WriteLine("👍 Config loaded.");

await using var db = new MySqlDataSource(config.GetDbConnString());

Console.WriteLine("🔄 Starting Sync Process...");

try
{
    // list mysql system users that should not ever be deleted
    var systemUsers = new HashSet<string>
    {
        "root",
        "mysql.session",
        "mysql.sys",
        "mariadb.sys",
        "mysql.infoschema",
        "healthcheck"
    };

    // delete old users (users not in env vars)
    await using (var cmd = db.CreateCommand("SELECT User FROM mysql.user"))
    await using (var reader = await cmd.ExecuteReaderAsync())
    {
        while (await reader.ReadAsync())
        {
            var dbUser = reader.GetString(0);

            if (systemUsers.Contains(dbUser))
                continue; // skip system/root users (we wont ever delete those)

            var roleExistsInConfig = config.Users
                .FirstOrDefault(user => user.Username == dbUser)
                is not null;

            if (roleExistsInConfig is false)
            {
                Console.WriteLine($"🗑️ Removing user: {dbUser}");
                await db
                    .CreateCommand($"DROP USER '{dbUser}'@'%'")
                    .ExecuteNonQueryAsync();
            }
        }
    }

    // update users and their databases
    foreach (var user in config.Users)
    {
        var userPass = user.Password;

        // user
        var exists = await db
            .CreateCommand($"SELECT 1 FROM mysql.user WHERE User = '{user.Username}'")
            .ExecuteScalarAsync()
            is not null;

        if (!exists)
        {
            Console.WriteLine($"👤 Creating user: {user.Username}");
            await db
                .CreateCommand($"CREATE USER '{user.Username}'@'%' IDENTIFIED BY '{userPass}'")
                .ExecuteNonQueryAsync();
        }
        else
        {
            Console.WriteLine($"🔐 Updating password: {user.Username}");
            await db
                .CreateCommand($"ALTER USER '{user.Username}'@'%' IDENTIFIED BY '{userPass}'")
                .ExecuteNonQueryAsync();
        }

        // user's databases
        foreach (var dbName in user.Databases)
        {
            var dbExists = await db
                .CreateCommand($"SELECT 1 FROM information_schema.schemata WHERE SCHEMA_NAME = '{dbName}'")
                .ExecuteScalarAsync()
                is not null;

            if (!dbExists)
            {
                Console.WriteLine($"📦 Creating DB: {dbName}");
                await db
                    .CreateCommand($"CREATE DATABASE `{dbName}`")
                    .ExecuteNonQueryAsync();
            }

            await db
                .CreateCommand($"GRANT ALL PRIVILEGES ON `{dbName}`.* TO '{user.Username}'@'%'")
                .ExecuteNonQueryAsync();
        }

        // not stricly needed, but wont hurt
        await db
            .CreateCommand("FLUSH PRIVILEGES")
            .ExecuteNonQueryAsync();
    }
    Console.WriteLine("✨ Sync complete.");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error: {ex.Message}");
    Environment.Exit(1);
}