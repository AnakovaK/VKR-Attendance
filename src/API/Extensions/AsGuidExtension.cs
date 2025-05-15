using System.Runtime.CompilerServices;

using Grpc.Core;

namespace RTUAttendAPI.API.Extensions;

public static class AsGuidExtension
{
    /// <summary>
    /// Преобразует строку в <see cref="Guid"/>
    /// </summary>
    /// <param name="stringGuid">строковое представление</param>
    /// <param name="stringGuidExpression"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">Если строка не являлась GUID</exception>
    public static Guid AsGuid(this string stringGuid, [CallerArgumentExpression("stringGuid")] string stringGuidExpression = "")
    {
        return Guid.TryParse(stringGuid, out var id) ? id : throw new ArgumentException("value is not guid", stringGuidExpression);
    }
}
