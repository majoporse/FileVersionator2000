using DataLayer.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace DataLayer.Installers;

public static class DbInstaller
{
    public static IServiceCollection DbInstall(this IServiceCollection services)
    {
        string? dbJson = Environment.GetEnvironmentVariable("DB_FILE");
        if (dbJson == null)
        {
            throw new Exception("DB_FILE environment variable not set");
        }

        services.AddSingleton<IDatabase, Database>(provider => new Database(dbJson));
        return services;
    }
    
}