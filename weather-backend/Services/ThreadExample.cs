using System;
using System.Threading;

namespace weather_backend.Services
{
    public class ThreadExample
    {
        private bool isCompleted;
        public ThreadExample()
        {
        }

        public void CreateNewThread()
        {
            Thread newThread = new Thread(ForLoopThread);
            newThread.Start();
            newThread.Name = "new thread";

            Thread.CurrentThread.Name = "main thread";
            ForLoopThread();
        }

        private void ForLoopThread()
        {
            if (!isCompleted)
            {
                Console.WriteLine("hello world");
                this.isCompleted = true;
            }
            
        }
    }
}