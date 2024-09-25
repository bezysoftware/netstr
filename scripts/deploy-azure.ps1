[CmdletBinding()]
param (
  [Parameter(Mandatory=$true, HelpMessage="Your top level domain, e.g. 'myrelay.com'. The actual relay will be setup at 'relay.myrelay.com'")]
  [String] $domain,

  [Parameter(Mandatory=$true, HelpMessage="Your email will be used for SSL certificate renewal notifications (used by certbot)")]
  [String] $email,

  [Parameter(Mandatory=$true, HelpMessage="Admin username for your new VM (also used for SSH access)")]
  [String] $username,

  [String] $location = "northeurope",
  [Switch] $dev = $false
)

$dns = $domain -replace '\.','-'
$group = $dns
$vm = "$dns-vm"

Write-Output "You will be able to SSH into your VM ($vm) by 'ssh $username@$dns.$location.cloudapp.azure.com'"

az login

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

# setup nginx prod
az vm run-command invoke `
    --resource-group "$group" `
    --name "$vm" `
    --command-id RunShellScript `
    --scripts @setup-nginx.sh `
    --parameters "relay-$dns relay.$domain 8080 $email"

# optionally setup nginx dev
if ($dev -eq $true) {
  az vm run-command invoke `
    --resource-group "$group" `
    --name "$vm" `
    --command-id RunShellScript `
    --scripts @setup-nginx.sh `
    --parameters "relay-dev-$dns relay-dev.$domain 8081 $email"
}

