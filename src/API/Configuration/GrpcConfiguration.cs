using RtuTc.TandemSchedule;

namespace RTUAttendAPI.API.Configuration;

public static class DIHelper
{
    public static void AddGrpcClients(this IServiceCollection services, string connectionString)
    {
        services.AddGrpcClient<TandemSchedule.TandemScheduleClient>(client =>
        {
            client.Address = new Uri(connectionString);
        });
    }
}
