#!/bin/bash

if [ -z "$1" ] || [ -z "$2" ] || [ -z "$3" ] || [ -z "$4" ]; then
	echo "Required parameters are site_name, server_name, port and email"
	exit 1
fi

SITE_NAME=$1
SERVER_NAME=$2
PORT=$3
EMAIL=$4

# no easy way to escape $ in an interpolated string?
scheme='$scheme'
http_upgrade='$http_upgrade'
host='$host'
proxy_add_x_forwarded_for='$proxy_add_x_forwarded_for'

CONFIG=`cat <<-_EOT_
server {
  listen        80;
  server_name   ${SERVER_NAME};
  location / {
      proxy_pass         http://127.0.0.1:${PORT}/;
      proxy_http_version 1.1;
      proxy_set_header   Upgrade $http_upgrade;
      proxy_set_header   Connection "upgrade";
      proxy_set_header   Host $host;
      proxy_cache_bypass $http_upgrade;
      proxy_set_header   X-Forwarded-For $proxy_add_x_forwarded_for;
      proxy_set_header   X-Forwarded-Proto $scheme;
  }
}
_EOT_
`

echo $CONFIG | sudo tee /etc/nginx/sites-available/$SITE_NAME
sudo ln -s /etc/nginx/sites-available/$SITE_NAME /etc/nginx/sites-enabled/$SITE_NAME
sudo certbot --nginx -d $SERVER_NAME --email $EMAIL --non-interactive --agree-tos
sudo nginx -s reload