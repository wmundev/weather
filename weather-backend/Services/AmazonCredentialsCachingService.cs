using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using Microsoft.Extensions.Logging;
using weather_backend.Exceptions;

namespace weather_backend.Services
{
    public sealed class AmazonCredentialsCachingService : AWSCredentials, IDisposable
    {
        volatile ImmutableCredentials? token;
        private readonly IAmazonSecurityTokenService credentialsProvider;
        private readonly ILogger<AmazonCredentialsCachingService> _logger;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly Task _backgroundTask;

        public AmazonCredentialsCachingService(IAmazonSecurityTokenService _credentialsProvider, ILogger<AmazonCredentialsCachingService> logger)
        {
            credentialsProvider = _credentialsProvider;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // Start the background refresh task and properly observe exceptions
            _backgroundTask = Task.Run(async () => await RefreshCredentialsLoop(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
        }

        private async Task RefreshCredentialsLoop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogDebug("Refreshing AWS credentials from AssumeRole");
                    var durationTokenValidSeconds = 900;
                    // var creds = await credentialsProvider.AssumeRoleWithWebIdentityAsync(
                    //     new AssumeRoleWithWebIdentityRequest {
                    //         WebIdentityToken = "",
                    //         DurationSeconds = 9000, 
                    //         RoleArn = "arn:aws:iam::123456789:role/test-assume-role",
                    //         RoleSessionName = "test-assume-role" 
                    //     });
                    var creds = await credentialsProvider.AssumeRoleAsync(
                        new AssumeRoleRequest 
                        { 
                            DurationSeconds = durationTokenValidSeconds, 
                            RoleArn = "arn:aws:iam::123456789:role/test-assume-role", 
                            RoleSessionName = "test-assume-role" 
                        }, 
                        cancellationToken);
                    
                    token = new ImmutableCredentials(creds.Credentials.AccessKeyId, creds.Credentials.SecretAccessKey, creds.Credentials.SessionToken);
                    _logger.LogInformation("AWS credentials refreshed successfully");

                    await Task.Delay(TimeSpan.FromSeconds(durationTokenValidSeconds - 5), cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Credential refresh loop canceled");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error refreshing AWS credentials");
                    // Wait a bit before retrying on error
                    await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
                }
            }
        }

        public override ImmutableCredentials GetCredentials()
        {
            return token ?? throw new CredentialsNotAvailableException("AWS credentials are not available yet. Please wait for the initial credentials refresh.");
        }

        public override Task<ImmutableCredentials> GetCredentialsAsync()
        {
            return Task.FromResult(GetCredentials());
        }

        public void Dispose()
        {
            if (!_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
                try
                {
                    // Wait a reasonable time for the background task to complete
                    _backgroundTask.Wait(TimeSpan.FromSeconds(5));
                }
                catch (AggregateException)
                {
                    // Task was already canceled or faulted, which is expected
                }
                finally
                {
                    _cancellationTokenSource.Dispose();
                }
            }
        }
    }
}
