using Weather.CLI.Services.DynamicProgramming;

namespace Weather.CLI.UnitTests.Services.DynamicProgramming
{
    public sealed class LongestIncreasingSubsequenceProblemTest
    {
        [Fact]
        public void SolveSolution()
        {
            int[] problem = { 5, 2, 8, 6, 3, 6, 9, 5 };

            var result = LongestIncreasingSubsequenceProblem.SolveSolution(problem);

            Assert.Equal(4, result);
        }
    }
}
