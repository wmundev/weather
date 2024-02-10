using Weather.CLI.Services.DynamicProgramming;

namespace Weather.CLI.UnitTests.Services.DynamicProgramming
{
    public sealed class LongestIncreasingSubsequenceProblemTest
    {
        [Theory]
        [InlineData(new int[] {5, 2, 8, 6, 3, 6, 9, 5}, 4)]
        [InlineData(new int[] {3, 2, 6, 4, 5, 1}, 3)]
        public void SolveSolution(int[] problem, int expectedResult)
        {
            var result = LongestIncreasingSubsequenceProblem.SolveSolution(problem);

            Assert.Equal(expectedResult, result);
        }
    }
}
