variable "environment" {
  type    = string
  default = "prod"
}

variable "stack-name" {
  type    = string
  default = "weather-infra"
}

variable "app_secret_parameter_store_value" {
  type    = string
  sensitive = true
} 