# ### STAGE 1: Build ###

# # We label our stage as ‘builder’
# FROM node:8.9.1 as builder

# COPY package.json package-lock.json ./

# ## Storing node modules on a separate layer will prevent unnecessary npm installs at each build
# RUN npm i && mkdir /ng-app && cp -R ./node_modules ./ng-app

# WORKDIR /ng-app

# COPY . .

# ## Build the angular app in production mode and store the artifacts in dist folder
# RUN $(npm bin)/ng build --prod


### STAGE 2: Setup ###

FROM nginx:1.13.3

# RUN apt-get update && apt-get install -y mate-core \
#     mate-desktop-environment mate-notification-daemon \
#     gconf-service libnspr4 libnss3 fonts-liberation \
#     libappindicator1 libcurl3 fonts-wqy-microhei && \
#     apt-get autoclean && apt-get autoremove && \
#     rm -rf /var/lib/apt/lists/* && \
#     echo "mate-session" > /home/desktop/.xsession

RUN apt-get update && apt-get install -y openssh-server net-tools supervisor nano 

# RUN echo xfce4-session > ~/.xsession
# RUN rm -f /etc/xrdp/startwm.sh
# COPY startwm.sh /etc/xrdp

RUN mkdir /var/run/sshd && mkdir log
RUN useradd vuta && echo 'vuta:Echo@1927' | chpasswd
RUN echo adduser vuta sudo
RUN echo 'root:Echo@1927' | chpasswd
RUN sed -i 's/PermitRootLogin prohibit-password/PermitRootLogin yes/' /etc/ssh/sshd_config

# SSH login fix. Otherwise user is kicked off after login
RUN sed 's@session\s*required\s*pam_loginuid.so@session optional pam_loginuid.so@g' -i /etc/pam.d/sshd

ENV NOTVISIBLE "in users profile"
RUN echo "export VISIBLE=now" >> /etc/profile
## Copy our default nginx config
COPY nginx/default.conf /etc/nginx/conf.d/

## Remove default nginx website
RUN rm -rf /usr/share/nginx/html/*

COPY supervisord.conf /etc/supervisor/conf.d/supervisord.conf

COPY /dist/. /ng-app

CMD ["supervisord", "-c","/etc/supervisor/supervisord.conf"]