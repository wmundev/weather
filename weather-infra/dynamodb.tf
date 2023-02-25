resource "aws_dynamodb_table" "basic_dynamodb_table" {
  name           = "weather"
  billing_mode   = var.environment == "test" ? "PAY_PER_REQUEST" : "PROVISIONED"
  read_capacity  = var.environment == "test" ? 0 : 1
  write_capacity = var.environment == "test" ? 0 : 1
  hash_key       = "name"
#  range_key      = "GameTitle"

  attribute {
    name = "name"
    type = "S"
  }

#  attribute {
#    name = "GameTitle"
#    type = "S"
#  }

  #  attribute {
  #    name = "TopScore"
  #    type = "N"
  #  }

  #  ttl {
  #    attribute_name = "TimeToExist"
  #    enabled        = false
  #  }

  #  global_secondary_index {
  #    name               = "GameTitleIndex"
  #    hash_key           = "GameTitle"
  #    range_key          = "TopScore"
  #    write_capacity     = 10
  #    read_capacity      = 10
  #    projection_type    = "INCLUDE"
  #    non_key_attributes = ["UserId"]
  #  }

  tags = {
  }
}

#resource "aws_appautoscaling_target" "dynamodb_table_read_target" {
#  max_capacity       = 5
#  min_capacity       = 1
#  resource_id        = "table/${aws_dynamodb_table.basic_dynamodb_table.name}"
#  scalable_dimension = "dynamodb:table:ReadCapacityUnits"
#  service_namespace  = "dynamodb"
#}
#
#resource "aws_appautoscaling_policy" "dynamodb_table_read_policy" {
#  name               = "DynamoDBReadCapacityUtilization:${aws_appautoscaling_target.dynamodb_table_read_target.resource_id}"
#  policy_type        = "TargetTrackingScaling"
#  resource_id        = aws_appautoscaling_target.dynamodb_table_read_target.resource_id
#  scalable_dimension = aws_appautoscaling_target.dynamodb_table_read_target.scalable_dimension
#  service_namespace  = aws_appautoscaling_target.dynamodb_table_read_target.service_namespace
#
#  target_tracking_scaling_policy_configuration {
#    predefined_metric_specification {
#      predefined_metric_type = "DynamoDBReadCapacityUtilization"
#    }
#
#    target_value = 70
#  }
#}
#
#resource "aws_appautoscaling_target" "dynamodb_table_write_target" {
#  max_capacity       = 5
#  min_capacity       = 1
#  resource_id        = "table/${aws_dynamodb_table.basic_dynamodb_table.name}"
#  scalable_dimension = "dynamodb:table:WriteCapacityUnits"
#  service_namespace  = "dynamodb"
#}
#
#resource "aws_appautoscaling_policy" "dynamodb_table_write_policy" {
#  name               = "DynamoDBWriteCapacityUtilization:${aws_appautoscaling_target.dynamodb_table_write_target.resource_id}"
#  policy_type        = "TargetTrackingScaling"
#  resource_id        = aws_appautoscaling_target.dynamodb_table_write_target.resource_id
#  scalable_dimension = aws_appautoscaling_target.dynamodb_table_write_target.scalable_dimension
#  service_namespace  = aws_appautoscaling_target.dynamodb_table_write_target.service_namespace
#
#  target_tracking_scaling_policy_configuration {
#    predefined_metric_specification {
#      predefined_metric_type = "DynamoDBWriteCapacityUtilization"
#    }
#
#    target_value = 70
#  }
#}