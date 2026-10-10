namespace Weather.CLI.Services.LanguageExamples
{
    /// <summary>
    /// Declaring and invoking a custom delegate type. Moved from the API's <c>GET /api/delegates</c>.
    /// </summary>
    public sealed class DelegateExample
    {
        public delegate void PairHandler(int first, int second);

        public void InvokeWithPair(PairHandler handler)
        {
            handler(1, 2);
        }
    }
}
