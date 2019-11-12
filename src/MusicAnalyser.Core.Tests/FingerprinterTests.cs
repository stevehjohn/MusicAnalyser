using NUnit.Framework;

namespace MusicAnalyser.Core.Tests
{
    [TestFixture]
    public class FingerprinterTests
    {
        [Test]
        public void Test()
        {
            Fingerprinter.GetFingerprint("C:\\Users\\Stevo\\Music\\Iron Maiden\\Seventh Son of a Seventh Son\\02 Infinite Dreams.mp3");
        }
    }
}
