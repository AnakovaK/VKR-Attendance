using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using AttendDatabase;

using Grpc.Net.Client;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using RTUAttendAPI.API.Services.BackgroundSchedule;

using RtuTc.RtuAttend.App;

using Testcontainers.PostgreSql;

namespace Tests.Specs.Hooks;
[Binding]
public class SetupWebApplicationHooks
{
    private static readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder().Build();
    private static CustomWebApplicationFactory<Program> _webApp = default!;

    private readonly ScenarioContext _scenarioContext;
    private IServiceScope _serviceScope = default!;

    public SetupWebApplicationHooks(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [BeforeScenario]
    public void InjectHttpClientToContext()
    {
        var client = _webApp.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        _scenarioContext.ScenarioContainer.RegisterInstanceAs(client);
        var channel = GrpcChannel.ForAddress(client.BaseAddress!, new GrpcChannelOptions
        {
            HttpClient = client,
        });
        
        var elderClient = new ElderService.ElderServiceClient(channel);
        _scenarioContext.ScenarioContainer.RegisterInstanceAs(elderClient);
        
        var teacherClient= new TeacherService.TeacherServiceClient(channel);
        _scenarioContext.ScenarioContainer.RegisterInstanceAs(teacherClient);
        _serviceScope = _webApp.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        _scenarioContext.ScenarioContainer.RegisterInstanceAs(_serviceScope.ServiceProvider.GetRequiredService<AttendDbContext>());
    }


    [AfterScenario]
    public void CleanupScope()
    {
        _serviceScope.Dispose();
    }


    [BeforeTestRun]
    public static async Task SetupWebApp()
    {
        await _postgreSqlContainer.StartAsync();
        _webApp = new CustomWebApplicationFactory<Program>(_postgreSqlContainer.GetConnectionString());
    }

    [AfterTestRun]
    public static async Task Stop()
    {
        await _postgreSqlContainer.DisposeAsync();
    }
}
public class CustomWebApplicationFactory<TProgram>
    : WebApplicationFactory<TProgram> where TProgram : class
{
    private readonly string _postgresConnectionString;

    public CustomWebApplicationFactory(string postgresConnectionString)
    {
        _postgresConnectionString = postgresConnectionString;
    }
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {

        builder.ConfigureAppConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                { "ConnectionStrings:AttendPostgresDatabase", _postgresConnectionString }
            }!);
        });

        builder.ConfigureServices(services =>
        {
            var jobScheduler = services.Single(d => d.ServiceType == typeof(IBackgroundJobScheduler));
            services.Remove(jobScheduler);
            services.AddSingleton<IBackgroundJobScheduler, DoNothingJobScheduler>();
        });
    }
    /// <summary>
    /// Класс для отключения работ во время тестов. При тестировании самих работ - появится что-то вроде Moq или собственная реализация тандема специально для тестирования
    /// </summary>
    private class DoNothingJobScheduler : IBackgroundJobScheduler
    {
        public Task FillVisitingLogCompositionFromAcademicGroup(string groupTitle, Guid semesterId)
            => Task.CompletedTask;
    }
}
