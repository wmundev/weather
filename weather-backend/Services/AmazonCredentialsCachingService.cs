using System;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;

namespace weather_backend.Services
{
    public sealed class AmazonCredentialsCachingService : AWSCredentials, IDisposable
    {
        volatile ImmutableCredentials? token;
        private readonly IAmazonSecurityTokenService credentialsProvider;

        public AmazonCredentialsCachingService(IAmazonSecurityTokenService _credentialsProvider)
        {
            credentialsProvider = _credentialsProvider;
            Task.Run(new Action(async () =>
            {
                while (true)
                {
                    Console.WriteLine("getting creds");
                    var durationTokenValidSeconds = 900;
                    // var creds = await credentialsProvider.AssumeRoleWithWebIdentityAsync(
                    //     new AssumeRoleWithWebIdentityRequest {
                    //         WebIdentityToken = "",
                    //         DurationSeconds = 9000, 
                    //         RoleArn = "arn:aws:iam::123456789:role/test-assume-role",
                    //         RoleSessionName = "test-assume-role" 
                    //     });
                    var creds = await credentialsProvider.AssumeRoleAsync(new AssumeRoleRequest { DurationSeconds = durationTokenValidSeconds, RoleArn = "arn:aws:iam::123456789:role/test-assume-role", RoleSessionName = "test-assume-role" });
                    // Console.WriteLine("creds" + JsonSerializer.Serialize( creds));
                    token = new ImmutableCredentials(creds.Credentials.AccessKeyId, creds.Credentials.SecretAccessKey, creds.Credentials.SessionToken);

                    await Task.Delay(TimeSpan.FromSeconds(durationTokenValidSeconds - 5));
                }
            }));
        }

        public override ImmutableCredentials GetCredentials()
        {
            return token ?? throw new Exception("token is null");
        }

        public override Task<ImmutableCredentials> GetCredentialsAsync()
        {
            return Task.FromResult(GetCredentials());
        }

        public void Dispose()
        {
        }
    }
}
