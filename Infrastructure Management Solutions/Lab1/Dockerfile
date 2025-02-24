FROM nginx:alpine
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY app/ /usr/share/nginx/html/
LABEL com.centurylinklabs.watchtower.enable="true"
