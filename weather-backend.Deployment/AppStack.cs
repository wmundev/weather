// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using System.Linq;
using Amazon.CDK;
using Amazon.CDK.AWS.IAM;
using AWS.Deploy.Recipes.CDK.Common;
using Constructs;
using weather_backend.Deployment.Configurations;

namespace weather_backend.Deployment
{
    public class AppStack : Stack
    {
        private readonly Configuration _configuration;

        internal AppStack(Construct scope, IDeployToolStackProps<Configuration> props)
            : base(scope, props.StackName, props)
        {
            _configuration = props.RecipeProps.Settings;

            // Setup callback for generated construct to provide access to customize CDK properties before creating constructs.
            CDKRecipeCustomizer<Recipe>.CustomizeCDKProps += CustomizeCDKProps;

            // Create custom CDK constructs here that might need to be referenced in the CustomizeCDKProps. For example if
            // creating a DynamoDB table construct and then later using the CDK construct reference in CustomizeCDKProps to
            // pass the table name as an environment variable to the container image.

            // Create the recipe defined CDK construct with all of its sub constructs.
            var generatedRecipe = new Recipe(this, props.RecipeProps);

            // Create additional CDK constructs here. The recipe's constructs can be accessed as properties on
            // the generatedRecipe variable.
        }

        /// <summary>
        /// This method can be used to customize the properties for CDK constructs before creating the constructs.
        ///
        /// The pattern used in this method is to check to evnt.ResourceLogicalName to see if the CDK construct about to be created is one
        /// you want to customize. If so cast the evnt.Props object to the CDK properties object and make the appropriate settings.
        /// </summary>
        /// <param name="evnt"></param>
        private void CustomizeCDKProps(CustomizePropsEventArgs<Recipe> evnt)
        {
            // Example of how to customize the Beanstalk Environment.
            // 
            // if (string.Equals(evnt.ResourceLogicalName, nameof(evnt.Construct.BeanstalkEnvironment)))
            // {
            //     if (evnt.Props is CfnEnvironmentProps props)
            //     {
            //         Console.WriteLine("Customizing Beanstalk Environment");
            //     }
            // }
            
            // if (string.Equals(evnt.ResourceLogicalName, nameof(evnt.Construct.BeanstalkEnvironment)))
            // {
            //     if (evnt.Props is CfnEnvironmentProps props)
            //     {
            //         var optionSettingsArray = props.OptionSettings as IEnumerable<object>;
            //         optionSettingsArray.ToList().Add(new CfnEnvironment.OptionSettingProperty
            //         {
            //             OptionName = "ConfigDocument",
            //             Value = _configuration.SolutionStackName
            //         });
            //     }
            // }

            if (string.Equals(evnt.ResourceLogicalName, nameof(evnt.Construct.AppIAMRole)))
            {
                if (evnt.Props is RoleProps props)
                {
                    props.ManagedPolicies = props.ManagedPolicies.Concat(
                        new[]
                        {
                            ManagedPolicy.FromAwsManagedPolicyName("AmazonDynamoDBReadOnlyAccess")
                        }
                    ).ToArray();

                    const string parameterStoreInlinePolicyName = "weather-backend-app-parameter-store";
                    var parameterStoreInlinePolicy = new PolicyDocument(new PolicyDocumentProps
                    {
                        Statements = new[]
                        {
                            new PolicyStatement(new PolicyStatementProps
                            {
                                Actions = new[] { "ssm:DescribeParameters"},
                                Resources = new[] { "*" }
                            }),
                            new PolicyStatement(new PolicyStatementProps
                            {
                                Actions = new[] { "ssm:GetParameter" },
                                Resources = new[] { "arn:aws:ssm:us-east-1:775267081896:parameter/weather_secrets" }
                            })
                        }
                    });

                    if (props.InlinePolicies != null)
                    {
                        props.InlinePolicies.Add(parameterStoreInlinePolicyName, parameterStoreInlinePolicy);
                    }
                    else
                    {
                        props.InlinePolicies = new Dictionary<string, PolicyDocument> { { parameterStoreInlinePolicyName, parameterStoreInlinePolicy } };
                    }
                }
            }
        }
    }
}