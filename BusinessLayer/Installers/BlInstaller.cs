using BusinessLayer.Services;
using DataLayer.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace BusinessLayer.Installers;

public static class BlInstaller
{
    public static IServiceCollection BlInstall(this IServiceCollection services)
    {
        string? dir = Environment.GetEnvironmentVariable("TRACKING_DIRECTORY");
        if (dir == null)
        {
            throw new Exception("TRACKING_DIRECTORY environment variable not set");
        }

        services.AddSingleton<IFileService, FileService>(provider =>
        {
            var database = provider.GetRequiredService<IDatabase>();
            return new FileService(database, dir);
        });
        return services;
    }
}