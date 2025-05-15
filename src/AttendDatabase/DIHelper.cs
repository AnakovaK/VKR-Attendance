using Ardalis.Specification;

using AttendDatabase;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using RTUAttendAPI.AttendDatabase.Models;
using RTUAttendAPI.AttendDatabase.Repositories;

namespace RTUAttendAPI.AttendDatabase;
public static class DIHelper
{
    public static void AddAttendDb(this IServiceCollection services, Func<string> connectionStringProvider)
    {
        services.AddSingleton((provider) =>
        {
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionStringProvider());

            dataSourceBuilder.MapEnum<StudentMembershipType>();
            dataSourceBuilder.MapEnum<StudentMembershipRole>();
            dataSourceBuilder.MapEnum<AuthorType>();
            dataSourceBuilder.MapEnum<AttendType>();
            dataSourceBuilder.MapEnum<VisitingLogSource>();

            return dataSourceBuilder.Build();
        });
        services.AddDbContext<AttendDbContext>(config => config
            .UseNpgsql(o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery))
        );

        services.AddScoped(typeof(IRepositoryBase<>), typeof(AttendRepository<>));
    }
}
