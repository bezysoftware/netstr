az login

$group = "netstr"
$vm = "netstr-vm"
$username = "bezysoftware"

# create resource group
az group create `
    --location northeurope `
    --name "$group"

# create vm
az vm create `
    --resource-group "$group" `
    --name "$vm" `
    --image Ubuntu2204 `
    --authentication-type ssh `
    --ssh-key-values ~/.ssh/id_rsa.pub `
    --data-disk-sizes-gb 128 `
    --size Standard_B2s `
    --public-ip-address-dns-name "$group" `
    --admin-username "$username"

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
    --run-as-user "$username"
    --scripts @setup-host.sh