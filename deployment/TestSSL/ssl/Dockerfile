FROM nginx:1.23.2-alpine
COPY ssl/ssl.conf.template /etc/nginx/templates/default.conf.template
COPY ../links.html /usr/share/nginx/html/index.html
EXPOSE 80
EXPOSE 443
