
resource "digitalocean_firewall" "ims_labs_vm_firewall" {
  name = "IMS-Labs-VM-Firewall"
  droplet_ids = [
    digitalocean_droplet.ims-labs-vm.id
  ]

  # Access to the droplet from anywhere, allow incoming SSH traffic (port 22)
  inbound_rule {
    protocol         = "tcp"
    port_range       = "22"
    source_addresses = ["0.0.0.0/0", "::/0"]
  }

  # Access to the droplet from anywhere, allow incoming HTTP traffic (port 80)
  inbound_rule {
    protocol         = "tcp"
    port_range       = "80"
    source_addresses = ["0.0.0.0/0", "::/0"]
  }

  # Access to the droplet from anywhere, allow incoming HTTPS traffic (port 443)
  inbound_rule {
    protocol         = "tcp"
    port_range       = "443"
    source_addresses = ["0.0.0.0/0", "::/0"]
  }

  # Allow all outbound traffic
  outbound_rule {
    protocol              = "tcp"
    port_range            = "all"
    destination_addresses = ["0.0.0.0/0", "::/0"]
  }
}