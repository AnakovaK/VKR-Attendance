namespace RTUAttendAPI.API.Configuration;

public static class DistributedCacheConfigure
{
    public static void AddDistributedCache(this WebApplicationBuilder builder)
    {
        var redisConnectionString = builder.Configuration.GetConnectionString(ConnectionStrings.RedisConnectionString);
        if (string.IsNullOrEmpty(redisConnectionString))
        {
            builder.Services.AddDistributedMemoryCache(); // TODO: добавить redis как второй способ кеша
        }
        else
        {
            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = "RTU-Attendance-API";
            });
        }

    }
}
