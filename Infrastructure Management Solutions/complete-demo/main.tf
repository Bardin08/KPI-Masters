terraform {
  required_providers {
    hcloud = {
      source  = "hetznercloud/hcloud"
      version = "~> 1.45"
    }
  }
  required_version = ">= 1.0"

  backend "s3" {
    bucket                      = "eb82bcceb8f8"
    region                      = "eu-north-1"
    skip_credentials_validation = true
    skip_region_validation      = true
    force_path_style            = true
  }
}

provider "hcloud" {
  token = var.hcloud_token
}

variable "hcloud_token" {
  description = "Hetzner Cloud API Token"
  type        = string
  sensitive   = true
}

variable "ssh_public_key" {
  description = "Path to SSH public key"
  type        = string
  sensitive   = true
}

variable "server_name" {
  description = "Name of the server"
  type        = string
  default     = "devops-demo-server"
}

variable "server_type" {
  description = "Server type"
  type        = string
  default     = "CX42"  # More resources for EFK stack
}

variable "location" {
  description = "Server location"
  type        = string
  default     = "nbg1"
}

# SSH Key
resource "hcloud_ssh_key" "default" {
  name = "devops-demo-key"
  public_key = var.ssh_public_key
}

# Firewall
resource "hcloud_firewall" "web_firewall" {
  name = "devops-demo-firewall"

  # SSH
  rule {
    direction = "in"
    protocol  = "tcp"
    port      = "22"
    source_ips = [
      "0.0.0.0/0",
      "::/0"
    ]
  }

  # HTTP
  rule {
    direction = "in"
    protocol  = "tcp"
    port      = "80"
    source_ips = [
      "0.0.0.0/0",
      "::/0"
    ]
  }

  # HTTPS
  rule {
    direction = "in"
    protocol  = "tcp"
    port      = "443"
    source_ips = [
      "0.0.0.0/0",
      "::/0"
    ]
  }

  # Grafana (if direct access needed)
  rule {
    direction = "in"
    protocol  = "tcp"
    port      = "3001"
    source_ips = [
      "0.0.0.0/0",
      "::/0"
    ]
  }

  # Kibana (if direct access needed)
  rule {
    direction = "in"
    protocol  = "tcp"
    port      = "5601"
    source_ips = [
      "0.0.0.0/0",
      "::/0"
    ]
  }
}

# Server
resource "hcloud_server" "web" {
  name        = var.server_name
  server_type = var.server_type
  image       = "ubuntu-22.04"
  location    = var.location
  ssh_keys = [hcloud_ssh_key.default.id]
  firewall_ids = [hcloud_firewall.web_firewall.id]

  user_data = templatefile("${path.module}/cloud-init.yaml", {
    docker_compose_content = base64encode(file("${path.module}/docker-compose.yaml"))
    nginx_config_content = base64encode(file("${path.module}/nginx/nginx.conf"))
    prometheus_config = base64encode(file("${path.module}/prometheus/prometheus.yml"))
    app_js_content = base64encode(file("${path.module}/app/app.js"))
    fluent_conf_content = base64encode(file("${path.module}/fluentd/fluent.conf"))
    fluentd_dockerfile = base64encode(file("${path.module}/fluentd/Dockerfile"))
  })

  lifecycle {
    create_before_destroy = true
  }
}

output "server_ip" {
  value = hcloud_server.web.ipv4_address
}

output "grafana_url" {
  value = "http://${hcloud_server.web.ipv4_address}/grafana/"
}

output "kibana_url" {
  value = "http://${hcloud_server.web.ipv4_address}/kibana/"
}

output "main_app_url" {
  value = "http://${hcloud_server.web.ipv4_address}/"
}