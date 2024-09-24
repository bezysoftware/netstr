az login

param (
  [switch] $dev = $false
)

$group = "netstr"
$vm = "netstr-vm"
$dns = $group
$username = "bezysoftware"
$location = "northeurope"

# create resource group
az group create `
    --location "$location" `
    --name "$group"

# create vm
az vm create `
    --resource-group "$group" `
    --name "$vm" `
    --image Ubuntu2204 `
    --authentication-type ssh `
    --ssh-key-values ~/.ssh/id_rsa.pub `
    --size Standard_B2s `
    --public-ip-address-dns-name "$dns" `
    --admin-username "$username"

# attach new disk
az vm disk attach `
    --resource-group "$group" `
    --vm-name "$vm" `
    --name "$vm-data" `
    --size-gb 128 `
    --new

# open ports 80 & 443
az vm open-port `
    --resource-group "$group" `
    --name "$vm" `
    --port 80,443 `
    --priority 100

# run init script on vm
az vm run-command invoke `
    --resource-group "$group" `
    --name "$vm" `
    --command-id RunShellScript `
    --scripts @setup-host.sh `
    --parameters "$username"

# wait for SSH to become available
sleep 30s

# setup nginx prod
cat .\nginx-config-prod | ssh "$username@$dns.$location.cloudapp.azure.com" "sudo tee /etc/nginx/sites-available/netstr-prod"

az vm run-command invoke `
    --resource-group "$group" `
    --name "$vm" `
    --command-id RunShellScript `
    --scripts "sudo ln -s /etc/nginx/sites-available/netstr-prod /etc/nginx/sites-enabled/netstr-prod" `

# optionally setup nginx dev
if ($dev -eq $true) {
    cat .\nginx-config-dev | ssh "$username@$dns.$location.cloudapp.azure.com" "sudo tee /etc/nginx/sites-available/netstr-dev"
    
    az vm run-command invoke `
        --resource-group "$group" `
        --name "$vm" `
        --command-id RunShellScript `
        --scripts "sudo ln -s /etc/nginx/sites-available/netstr-dev /etc/nginx/sites-enabled/netstr-dev" `
}

# reload nginx config
az vm run-command invoke `
    --resource-group "$group" `
    --name "$vm" `
    --command-id RunShellScript `
    --scripts "sudo nginx -s reload"`
