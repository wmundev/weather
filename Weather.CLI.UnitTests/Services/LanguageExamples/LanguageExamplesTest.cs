using Weather.CLI.Services.LanguageExamples;
using Weather.CLI.UnitTests.TestHelpers;

namespace Weather.CLI.UnitTests.Services.LanguageExamples
{
    public class LanguageExamplesTest
    {
        [Fact]
        public void InvokeWithPair_WhenCalled_ShouldPassOneAndTwo()
        {
            var received = (0, 0);

            new DelegateExample().InvokeWithPair((first, second) => received = (first, second));

            Assert.Equal((1, 2), received);
        }

        [Fact]
        public void RenameCopy_WhenCalled_ShouldLeaveTheCallersStructUnchanged()
        {
            var original = new StructCopyExample.Person {Name = "original", Age = 11};

            var renamed = StructCopyExample.RenameCopy(original, "renamed");

            Assert.Equal("original", original.Name);
            Assert.Equal("renamed", renamed.Name);
        }

        [Fact]
        public void RunOnTwoThreads_WhenCalled_ShouldLogFromBothThreads()
        {
            var logger = new RecordingLogger<ThreadExample>();

            new ThreadExample(logger).RunOnTwoThreads();

            Assert.Equal(2, logger.Messages.Count);
            Assert.Contains(logger.Messages, message => message.Contains("new thread", StringComparison.Ordinal));
        }

        [Fact]
        public void Builder_WhenEveryChannelIsSet_ShouldCreateTheMatchingColor()
        {
            var color = new Color.Builder().Red(1).Green(2).Blue(3).Alpha(4).Create();

            Assert.Equal((1, 2, 3, 4), (color.Red, color.Green, color.Blue, color.Alpha));
        }
    }
}
