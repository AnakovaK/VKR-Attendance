using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using System.Security.Claims;

namespace RTUAttendAPI.API.Authorization.MIREA;

public class MIREAOptions : OAuthOptions
{
    public MIREAOptions()
    {
        CallbackPath = new PathString("/api/mireaauth"); // будет обработано самим фреймворком
        AccessDeniedPath = new PathString("/auth_error"); // фронтенд, расположенный на том же домене, что и API, должен обработать по этому пути UI, где расскажет, что произошла ошибка

        Scope.Add("basic");

        // Запрашиваются все scope для корректного сообщения о получаемой информации. Фактически информация будет взята из ТАНДЕМ
        Scope.Add("student");
        Scope.Add("employee");

        ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "uid");
        ClaimActions.MapJsonKey(ClaimTypes.Name, "username");
        ClaimActions.MapJsonKey(ClaimTypes.GivenName, "name");
        ClaimActions.MapJsonKey(ClaimTypes.Surname, "lastname");
        ClaimActions.MapJsonKey(SpecialClaims.Patronymic, "middlename");

        ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
    }

    public override void Validate()
    {
        base.Validate();
    }
}
