variable "do_token" {
  description = "DigitalOcean API token"
  type        = string
}

variable "ssh_public_key" {
  description = "SSH public key for accessing the droplet"
  type        = string
}

variable "ssh_private_key_path" {
  description = "SSH private key for accessing the droplet"
  type        = string
}

variable "ssh_key_fingerprint" {
  description = "SSH key fingerprint"
  type        = string
}