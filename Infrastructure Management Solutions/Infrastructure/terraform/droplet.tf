resource "digitalocean_ssh_key" "ims_labs_vm_ssh" {
  name       = "IMS Labs SSH Key"
  public_key = var.ssh_public_key
}

resource "digitalocean_droplet" "ims-labs-vm" {
  image   = "ubuntu-22-04-x64"
  name    = "IMS-Labs-VM"
  region  = "ams3"
  size    = "s-1vcpu-512mb-10gb"
  backups = false

  ssh_keys = [
    digitalocean_ssh_key.ims_labs_vm_ssh.id
  ]
}
