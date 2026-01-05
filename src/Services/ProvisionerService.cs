
namespace DbProvisioner.Services;

public class ProvisionerService(ConfigService config, IMySqlDatabaseService db)
{
    public async Task SyncDatabase()
    {
        try
        {
            var systemUsers = new HashSet<string>
            {
                "root",
                "mysql.session",
                "mysql.sys",
                "mariadb.sys",
                "mysql.infoschema",
                "healthcheck"
            };

            // cleanup old users
            var existingUsers = await db.GetUsersAsync();
            foreach (var dbUser in existingUsers)
            {
                if (systemUsers.Contains(dbUser))
                    continue;

                if (!config.Users.Any(u => u.Username == dbUser))
                {
                    Console.WriteLine($"🗑️ Removing user: {dbUser}");
                    await db.ExecuteNonQueryAsync($"DROP USER '{dbUser}'@'%'");
                }
            }

            // provision users and DBs
            foreach (var user in config.Users)
            {
                if (!await db.UserExistsAsync(user.Username))
                {
                    Console.WriteLine($"👤 Creating user: {user.Username}");
                    await db.ExecuteNonQueryAsync($"CREATE USER '{user.Username}'@'%' IDENTIFIED BY '{user.Password}'");
                }
                else
                {
                    Console.WriteLine($"🔐 Updating password: {user.Username}");
                    await db.ExecuteNonQueryAsync($"ALTER USER '{user.Username}'@'%' IDENTIFIED BY '{user.Password}'");
                }

                foreach (var dbName in user.Databases)
                {
                    if (!await db.DatabaseExistsAsync(dbName))
                    {
                        Console.WriteLine($"📦 Creating DB: {dbName}");
                        await db.ExecuteNonQueryAsync($"CREATE DATABASE `{dbName}`");
                    }
                    await db.ExecuteNonQueryAsync($"GRANT ALL PRIVILEGES ON `{dbName}`.* TO '{user.Username}'@'%'");
                }
            }
            await db.ExecuteNonQueryAsync("FLUSH PRIVILEGES");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            throw;
        }
    }
}