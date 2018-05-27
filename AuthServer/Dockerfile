FROM microsoft/aspnetcore:2.0 AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/aspnetcore-build:2.0 AS build
WORKDIR /src
COPY AuthServer/AuthServer.csproj AuthServer/
COPY Core/VDS.Common/VDS.Common.csproj Core/VDS.Common/
COPY Core/VDS.IdentityServer4.EntityFrameworkCore/VDS.IdentityServer4.EntityFrameworkCore.csproj Core/VDS.IdentityServer4.EntityFrameworkCore/
COPY Core/VDS.Storage.EntityFrameworkCore/VDS.Storage.EntityFrameworkCore.csproj Core/VDS.Storage.EntityFrameworkCore/
COPY Core/VDS/VDS.csproj Core/VDS/
COPY Core/VDS.IdentityServer4/VDS.IdentityServer4.csproj Core/VDS.IdentityServer4/
COPY Core/VDS.BackgroundJobs.Hangfire/VDS.BackgroundJobs.Hangfire.csproj Core/VDS.BackgroundJobs.Hangfire/
COPY Core/VDS.AspNetCore/VDS.AspNetCore.csproj Core/VDS.AspNetCore/
RUN dotnet restore AuthServer/AuthServer.csproj
COPY . .
WORKDIR /src/AuthServer
RUN dotnet build AuthServer.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish AuthServer.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "AuthServer.dll"]
