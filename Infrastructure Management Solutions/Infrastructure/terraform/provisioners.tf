
resource "local_file" "ansible_inventory" {
  filename = "../ansible/inventory"
  content  = <<-EOT
    [droplets]
    ${digitalocean_droplet.ims-labs-vm.ipv4_address} ansible_ssh_user=root ansible_ssh_private_key_file=${var.ssh_private_key_path}
  EOT
}

resource "null_resource" "update_known_hosts" {
  provisioner "local-exec" {
    command = <<EOT
      chmod +x ./scripts/update_known_hosts.sh
      ./scripts/update_known_hosts.sh ${digitalocean_droplet.ims-labs-vm.ipv4_address}
    EOT
  }

  depends_on = [
    digitalocean_droplet.ims-labs-vm
  ]
}

resource "null_resource" "ansible_provisioner" {
  triggers = {
    always_run = timestamp()
  }

  provisioner "local-exec" {
    command = <<EOT
      ansible-playbook -i ../ansible/inventory ../ansible/base_setup_playbook.yml
      ansible-playbook -i ../ansible/inventory ../ansible/install_ims_app_playbook.yml
      ansible-playbook -i ../ansible/inventory ../ansible/install_watchtower_playbook.yml
    EOT
  }

  depends_on = [
    digitalocean_droplet.ims-labs-vm,
    local_file.ansible_inventory,
    null_resource.update_known_hosts
  ]
}