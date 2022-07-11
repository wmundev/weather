terraform {
  backend "s3" {
    bucket = "terraform-9b9eff0a-1fcb-46f1-b970-267d3760c8f3"
    key    = "weather-infra"
    region = "us-east-1"
  }

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 4.22.0"
    }
  }
}

provider "aws" {
  profile = "default"
  region  = "us-east-1"

  default_tags {
    tags = {
      environment = var.environment
      stack-name = var.stack-name
    }
  }
}

