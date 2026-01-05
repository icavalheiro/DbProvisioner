using DbProvisioner.Services;

Console.WriteLine("🔄 Loading config env vars...");
var env = new EnvironmentService();

var config = new ConfigService(env);
Console.WriteLine("👍 Config loaded.");

var db = new MySqlDatabaseService(config.GetDbConnString());
var service = new ProvisionerService(config, db);

Console.WriteLine("🔄 Starting Sync Process...");

await service.SyncDatabase();

Console.WriteLine("✨ Sync complete.");

