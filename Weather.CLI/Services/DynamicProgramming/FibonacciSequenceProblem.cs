namespace Weather.CLI.Services.DynamicProgramming
{
    public sealed class FibonacciSequenceProblem
    {
        public static long SolveProblem(long problemIndex)
        {
            const long firstValue = 1;
            const long secondValue = 1;
            if (problemIndex is 1 or 2)
            {
                return firstValue;
            }

            var memoMap = new Dictionary<long, long>();
            memoMap.Add(1, firstValue);
            memoMap.Add(2, secondValue);

            long solutionIndex = 2;
            long result = 0;
            do
            {
                solutionIndex++;
                result = memoMap[solutionIndex - 1] + memoMap[solutionIndex - 2];
                memoMap.Add(solutionIndex, result);
            } while (solutionIndex != problemIndex);

            return result;
        }
    }
}
