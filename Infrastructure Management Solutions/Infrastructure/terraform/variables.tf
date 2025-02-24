variable "do_token" {
  description = "DigitalOcean API token"
  type        = string
}

variable "ssh_public_key" {
  description = "SSH public key for accessing the droplet"
  type        = string
}