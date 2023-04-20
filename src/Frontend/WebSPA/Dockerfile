# build environment
FROM node:alpine as build
WORKDIR /app
ENV PATH /app/node_modules/.bin:$PATH
COPY Frontend/WebSPA/page/package.json ./
COPY Frontend/WebSPA/page/package-lock.json ./
ARG BASE_URL
ARG TOKEN_SERVICE_URL
ARG OIDC_CLIENT_SECRET
RUN npm ci --silent
RUN npm install react-scripts@3.4.1 -g --silent
COPY ./Frontend/WebSPA/page ./
RUN REACT_APP_API_URL=$BASE_URL REACT_APP_TOKEN_SERVICE_URL=$TOKEN_SERVICE_URL REACT_APP_OIDC_CLIENT_SECRET=$OIDC_CLIENT_SECRET npm run build

# production environment
FROM nginx:stable-alpine
COPY --from=build /app/build /usr/share/nginx/html
RUN mkdir -p /usr/share/nginx/html/ref/
COPY Frontend/WebSPA/nginx/links.html /usr/share/nginx/html/ref/index.html
      
COPY srv/nginx/conf/security.conf /etc/nginx/security.conf
COPY srv/nginx/conf/proxy.conf /etc/nginx/proxy.conf

EXPOSE 80
EXPOSE 443