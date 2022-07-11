namespace weather_backend.Services
{
    public class DelegateService
    {
        public delegate void TestDelegate(int a, int b);

        public void SortThings(TestDelegate someDelegate)
        {
            someDelegate(1, 2);
        }
    }
}