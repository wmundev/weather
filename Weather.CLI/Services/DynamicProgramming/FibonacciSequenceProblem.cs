namespace Weather.CLI.Services.DynamicProgramming
{
    public sealed class FibonacciSequenceProblem
    {
        public static int SolveProblem(int haha)
        {
            var problemIndex = haha;
            int firstValue = 1;
            int secondValue = 1;
            if (problemIndex == 1 || problemIndex == 2)
            {
                return firstValue;
            }

            var memoMap = new Dictionary<int, int>();
            memoMap.Add(1, firstValue);
            memoMap.Add(2, secondValue);

            int solutionIndex = 2;
            int result = 0;
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
