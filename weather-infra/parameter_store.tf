resource "aws_ssm_parameter" "app_secret" {
  name        = "weather_secrets"
  description = "Secrets for the weather app in json"
  type        = "SecureString"
  value       = var.app_secret_parameter_store_value
}