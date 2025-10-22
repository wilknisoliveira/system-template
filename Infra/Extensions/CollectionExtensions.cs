using Infra.Context.Repositories;
using Infra.Context.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;
using Shared.Domain;

namespace Infra.Extensions;

public static class CollectionExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }
}