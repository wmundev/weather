using Xunit;

// need to be disabled because test run forever in CICD without disabling parallelization
[assembly: CollectionBehavior(DisableTestParallelization = true)]
