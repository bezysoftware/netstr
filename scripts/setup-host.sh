#!/bin/bash

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

# Setup docker
sudo groupadd docker
sudo usermod -aG docker $USER
newgrp docker

# Install certbot
sudo snap install --classic certbot
sudo ln -s /snap/bin/certbot /usr/bin/certbot

# Partition data disk - this assumes the data drive is named "sda"
sudo parted /dev/sda --script mklabel gpt mkpart xfspart xfs 0% 100%
sudo mkfs.xfs /dev/sda1
sudo partprobe /dev/sda1

# Create mount folder
sudo mkdir /data
sudo mkdir /data/postgres

# Mount
sudo mount /dev/sda1 /data/postgres

# Append to fstab to mount after reboot
grep data/postgres /etc/mtab | sudo tee -a /etc/fstab