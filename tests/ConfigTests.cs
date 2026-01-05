using Xunit;
using Moq;
using DbProvisioner.Services;
using System.Collections;

public class ConfigTests
{
    [Fact]
    public void Config_ParsesDatabaseUsers_FromEnvironment()
    {
        // Arrange
        var mockEnv = new Mock<IEnvironmentService>();
        var envVars = new Hashtable
        {
            { "DB_APP1_USERNAME", "user1" }
        };

        mockEnv.Setup(e => e.GetEnvironmentVariables()).Returns(envVars);
        mockEnv.Setup(e => e.GetEnvironmentVariable("DB_APP1_USERNAME")).Returns("user1");
        mockEnv.Setup(e => e.GetEnvironmentVariable("DB_APP1_PASSWORD")).Returns("pass1");
        mockEnv.Setup(e => e.GetEnvironmentVariable("DB_HOST")).Returns("localhost");

        // Act
        var config = new ConfigService(mockEnv.Object);

        // Assert
        Assert.Single(config.Users);
        Assert.Equal("user1", config.Users[0].Username);
        Assert.Equal("pass1", config.Users[0].Password);
        Assert.Equal("APP1", config.Users[0].Databases[0]);
    }

    [Fact]
    public void GetDbConnString_ReturnsCorrectFormat()
    {
        // Arrange
        var mockEnv = new Mock<IEnvironmentService>();
        mockEnv.Setup(e => e.GetEnvironmentVariables()).Returns(new Hashtable());
        mockEnv.Setup(e => e.GetEnvironmentVariable("DB_HOST")).Returns("myserver");

        var config = new ConfigService(mockEnv.Object);

        // Act
        var connectionString = config.GetDbConnString("testdb");

        // Assert
        Assert.Contains("Server=myserver;", connectionString);
        Assert.Contains("Database=testdb;", connectionString);
    }
}