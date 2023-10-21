using Weather.CLI.Services.DynamicProgramming;

namespace Weather.CLI.UnitTests.Services.DynamicProgramming
{
    public sealed class FibonacciSequenceProblemTest
    {
        [Theory]
        [InlineData(1, 1)]
        [InlineData(2, 1)]
        [InlineData(3, 2)]
        [InlineData(4, 3)]
        [InlineData(5, 5)]
        [InlineData(6, 8)]
        [InlineData(7, 13)]
        [InlineData(8, 21)]
        [InlineData(9, 34)]
        [InlineData(50, 12586269025)]
        private void SolveProblem(long problem, long solution)
        {
            // int[] problem = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            // int[] solution = new[] { 1, 1, 2, 3, 5, 8, 13, 21, 34 };

            var result = FibonacciSequenceProblem.SolveProblem(problem);
            Assert.Equal(solution, result);
        }
    }
}
