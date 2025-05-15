FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 5025
EXPOSE 5026
ENV ASPNETCORE_URLS ""

RUN apt update && apt install -y curl
HEALTHCHECK CMD curl --fail http://localhost:5025/healthcheck || exit

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /build
COPY ["src/API/API.csproj", "src/API/"]
COPY ["src/AttendDatabase/AttendDatabase.csproj", "src/AttendDatabase/"]
COPY ["src/Proto/Proto.csproj", "src/Proto/"]
RUN dotnet restore "src/API/API.csproj"
COPY . .
WORKDIR "/build/src/API"
RUN dotnet build "API.csproj" -c Release -o /app/build --no-restore

FROM build AS publish
ARG VERSION
ENV VERSION=${VERSION:-1.0.0}
ARG SENTRY_AUTH_TOKEN
ENV SENTRY_AUTH_TOKEN=$SENTRY_AUTH_TOKEN
RUN dotnet publish "API.csproj" -c Release -o /app/publish /p:Version=$VERSION /p:UseAppHost=false --no-restore

FROM base AS final

ARG VERSION
ENV SENTRY_RELEASE=${VERSION:-1.0.0}

COPY --from=publish /app/publish .
CMD ["dotnet", "API.dll"]