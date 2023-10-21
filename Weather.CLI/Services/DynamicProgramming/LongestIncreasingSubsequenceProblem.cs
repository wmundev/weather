namespace Weather.CLI.Services.DynamicProgramming
{
    public sealed class LongestIncreasingSubsequenceProblem
    {
        /**
         * https://www.youtube.com/watch?v=aPQY__2H3tE
         * Find an appropriate subproblem
         * Find relationships among subproblems
         * Generalize the relationship
         * Implement by solving subproblems in order
         */
        public static int SolveSolution(int[] problem)
        {
            var result = 1;

            for (int i = 0; i < problem.Length; i++)
            {
                int maxSubsequence = 1;
                for (int j = 0; j < i; j++)
                {
                    int newLengthOfSubsequence = 1;
                    if (problem[j] < problem[i])
                    {
                        newLengthOfSubsequence++;
                    }

                    maxSubsequence = Math.Max(maxSubsequence, newLengthOfSubsequence);
                }

                Console.WriteLine("maxSubsequence " + maxSubsequence);
                result = Math.Max(maxSubsequence, result);
                Console.WriteLine($"i: {i} nice result {result}");
            }

            return result;
        }
    }
}
