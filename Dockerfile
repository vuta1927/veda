FROM vuta/ubuntu_netcore_nodejs:latest as build

RUN mkdir app && mkdir app/vds && mkdir app/vds/ApiServer && app/vds/AuthServer && app/vds/vds_client

COPY /ApiServer/bin/Debug/netcoreapp2.0/. app/vds/ApiServer
COPY /AuthServer/bin/Debug/netcoreapp2.0/. app/vds/AuthServer

RUN apt-get update

# RUN curl --silent --location https://deb.nodesource.com/setup_8.x | sudo bash -
# RUN apt-get install -y nodejs
# RUN apt-get install -y build-essential
# RUN apt-get install -y nginx
## Remove default nginx website
RUN rm -rf /usr/share/nginx/html/*
COPY Client/nginx/default.conf /etc/nginx/conf.d/
COPY Client/dist/. app/vds/vds_client

EXPOSE 52000

COPY Client/supervisor.conf /etc/supervisor.conf
CMD ["supervisord", "-c", "/etc/supervisor.conf"]
