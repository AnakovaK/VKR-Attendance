namespace RTUAttendAPI.API.Configuration;

public class CreateLessonFromScheduleOptions
{
    /// <summary>
    /// Период времени, который должен быть просканирован во время создания занятий
    /// </summary>
    public TimeSpan RangeToScan { get; set; }
}
