FROM microsoft/aspnetcore:2.0 AS base
WORKDIR /app
EXPOSE 80 22

FROM microsoft/aspnetcore-build:2.0 AS build
WORKDIR /src
COPY ApiServer/ApiServer.csproj ApiServer/
COPY Core/VDS.Common/VDS.Common.csproj Core/VDS.Common/
COPY Core/VDS.IdentityServer4.EntityFrameworkCore/VDS.IdentityServer4.EntityFrameworkCore.csproj Core/VDS.IdentityServer4.EntityFrameworkCore/
COPY Core/VDS.Storage.EntityFrameworkCore/VDS.Storage.EntityFrameworkCore.csproj Core/VDS.Storage.EntityFrameworkCore/
COPY Core/VDS/VDS.csproj Core/VDS/
COPY Core/VDS.IdentityServer4/VDS.IdentityServer4.csproj Core/VDS.IdentityServer4/
COPY Core/VDS.AspNetCore/VDS.AspNetCore.csproj Core/VDS.AspNetCore/
RUN dotnet restore ApiServer/ApiServer.csproj
COPY . .
WORKDIR /src/ApiServer
RUN dotnet build ApiServer.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish ApiServer.csproj -c Release -o /app

FROM base AS final
RUN apt-get update && apt-get install -y openssh-server supervisor wget iputils-ping
RUN mkdir /var/run/sshd
RUN useradd vuta && echo 'vuta:Echo@1927' | chpasswd
RUN echo adduser vuta sudo
RUN echo 'root:Echo@1927' | chpasswd
RUN sed -i 's/PermitRootLogin prohibit-password/PermitRootLogin yes/' /etc/ssh/sshd_config

# SSH login fix. Otherwise user is kicked off after login
RUN sed 's@session\s*required\s*pam_loginuid.so@session optional pam_loginuid.so@g' -i /etc/pam.d/sshd

ENV NOTVISIBLE "in users profile"
RUN echo "export VISIBLE=now" >> /etc/profile

COPY ApiServer/supervisord.conf /etc/supervisor/conf.d/supervisord.conf

WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["supervisord", "-c","/etc/supervisor.conf"]
