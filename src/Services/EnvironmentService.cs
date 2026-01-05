using System.Collections;

namespace DbProvisioner.Services;

public interface IEnvironmentService
{
    IDictionary GetEnvironmentVariables();
    string? GetEnvironmentVariable(string name);
}

public class EnvironmentService : IEnvironmentService
{
    public IDictionary GetEnvironmentVariables() => Environment.GetEnvironmentVariables();
    public string? GetEnvironmentVariable(string name) => Environment.GetEnvironmentVariable(name);
}