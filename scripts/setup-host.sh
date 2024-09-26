#!/bin/bash

if [ -z "$1" ]; then
	echo "Username parameter is required"
	exit 1
fi

username=$1

# Add Docker's official GPG key:
sudo apt-get update
sudo apt-get install ca-certificates curl gnupg
sudo install -m 0755 -d /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg
sudo chmod a+r /etc/apt/keyrings/docker.gpg

# Add the repository to Apt sources:
echo \
  "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu \
  $(. /etc/os-release && echo "$VERSION_CODENAME") stable" | \
  sudo tee /etc/apt/sources.list.d/docker.list > /dev/null
sudo apt-get update

# Install docker & nginx
sudo apt-get --yes install docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin nginx

# Remove default nginx page
sudo rm /etc/nginx/sites-enabled/default

# Setup docker for given user to run without sudo
sudo usermod -aG docker $username
newgrp docker

# Install certbot
sudo snap install --classic certbot
sudo ln -s /snap/bin/certbot /usr/bin/certbot

# Partition data disk - this assumes the data drive is named "sdc"
sudo parted /dev/sdc --script mklabel gpt mkpart xfspart xfs 0% 100%
sudo mkfs.xfs /dev/sdc1
sudo partprobe /dev/sdc1

# Create data folder
sudo mkdir -p /data/{dev,prod}/postgres
sudo mkdir -p /data/{dev,prod}/netstr/logs

# Mount
sudo mount /dev/sdc1 /data

# Make $username the owner of data folder (would be root otherwise)
sudo chown -R $username: /data
