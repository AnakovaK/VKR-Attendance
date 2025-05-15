namespace RTUAttendAPI.API.Configuration;

public static class ConnectionStrings
{
    public const string PostgresDatabase = "AttendPostgresDatabase";
    public const string QuartzDatabase = "QuartzDatabase";
    public const string BackgroundScheduleLink = "BackgroundScheduleLink";
    public const string GrpcTandem = "TandemGrpcEndpoint";

    public const string RedisConnectionString = "RedisCache";
}
