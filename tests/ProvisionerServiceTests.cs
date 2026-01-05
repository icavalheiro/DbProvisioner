using Moq;
using DbProvisioner.Services;

public class ProvisionerServiceTests
{
    [Fact]
    public async Task Sync_DeletesUser_WhenNotInConfig()
    {
        var mockDb = new Mock<IMySqlDatabaseService>();
        var mockEnv = new Mock<IEnvironmentService>();
        mockEnv.Setup(e => e.GetEnvironmentVariables()).Returns(new System.Collections.Hashtable());

        var config = new ConfigService(mockEnv.Object); // Empty users
        mockDb.Setup(d => d.GetUsersAsync()).ReturnsAsync(new List<string> { "old_user" });

        var service = new ProvisionerService(config, mockDb.Object);

        await service.SyncDatabase();

        mockDb.Verify(d => d.ExecuteNonQueryAsync(It.Is<string>(s => s.Contains("DROP USER 'old_user'"))), Times.Once);
    }

    [Theory]
    [InlineData("root")]
    [InlineData("mysql.session")]
    [InlineData("mysql.sys")]
    [InlineData("mariadb.sys")]
    [InlineData("mysql.infoschema")]
    [InlineData("healthcheck")]
    public async Task Sync_DoesNotDelete_SystemUsers(string systemUser)
    {
        // Arrange
        var mockDb = new Mock<IMySqlDatabaseService>();
        var config = new ConfigService(Mock.Of<IEnvironmentService>(e => e.GetEnvironmentVariables() == new System.Collections.Hashtable()));
        mockDb.Setup(d => d.GetUsersAsync()).ReturnsAsync(new List<string> { systemUser });

        var service = new ProvisionerService(config, mockDb.Object);

        // Act
        await service.SyncDatabase();

        // Assert
        mockDb.Verify(d => d.ExecuteNonQueryAsync(It.Is<string>(s => s.Contains("DROP USER"))), Times.Never);
    }

    [Theory]
    [InlineData("TESTDB", "new_user", "pass")]
    [InlineData("coliseo", "roman", "IVVL")]
    [InlineData("amazing_database_name", "admin", "pass123")]
    [InlineData("DB__NAME__USERNAME_12", "god", "123")]
    public async Task Sync_CreatesUser_WhenDoesNotExist(string dbName, string username, string pass)
    {
        // Arrange
        var mockEnv = new Mock<IEnvironmentService>();
        var envVars = new System.Collections.Hashtable { { $"DB_{dbName}_USERNAME", username } };
        mockEnv.Setup(e => e.GetEnvironmentVariables()).Returns(envVars);
        mockEnv.Setup(e => e.GetEnvironmentVariable($"DB_{dbName}_USERNAME")).Returns(username);
        mockEnv.Setup(e => e.GetEnvironmentVariable($"DB_{dbName}_PASSWORD")).Returns(pass);

        var mockDb = new Mock<IMySqlDatabaseService>();
        mockDb.Setup(d => d.GetUsersAsync()).ReturnsAsync(new List<string>());
        mockDb.Setup(d => d.UserExistsAsync(username)).ReturnsAsync(false);

        var config = new ConfigService(mockEnv.Object);
        var service = new ProvisionerService(config, mockDb.Object);

        // Act
        await service.SyncDatabase();

        // Assert
        mockDb.Verify(d => d.ExecuteNonQueryAsync(It.Is<string>(s => s.Contains($"CREATE USER '{username}'"))), Times.Once);
    }
}