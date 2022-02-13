resource "aws_iam_user" "app_iam_user" {
  name = "weather_iam_user"
  path = "/system/"
}

resource "aws_iam_access_key" "app_access_key" {
  user = aws_iam_user.app_iam_user.name
}

resource "aws_iam_user_policy" "lb_ro" {
  name = "weather_iam_user_policy"
  user = aws_iam_user.app_iam_user.name

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = [
          "dynamodb:List*",
          "dynamodb:DescribeReservedCapacity*",
          "dynamodb:DescribeLimits",
          "dynamodb:DescribeTimeToLive",
          "dynamodb:BatchGet*",
          "dynamodb:DescribeStream",
          "dynamodb:DescribeTable",
          "dynamodb:Get*",
          "dynamodb:Query",
          "dynamodb:Scan",
          "dynamodb:BatchWrite*",
          "dynamodb:CreateTable",
          "dynamodb:Delete*",
          "dynamodb:Update*",
          "dynamodb:PutItem"
        ]
        Effect   = "Allow"
        Resource = aws_dynamodb_table.basic_dynamodb_table.arn
      },
    ]
  })
}