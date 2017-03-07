using NUnit.Framework;

namespace ServiceImport.Framework.Tests.Extension
{
    [TestFixture]
    public class PascalCaseFieldNamesExtensionTests
    {
        [Test]
        public void PrivateInstanceFieldsThatDoNotHavePascalCaseFieldNamesAreRenamed()
        {
        }

        [Test]
        public void ReferencesToPrivateInstanceFieldsThatDoNotHavePascalCaseFieldNamesAreUpdated()
        {
        }

        [Test]
        public void PublicInstanceFieldsThatDoNotHavePascalCaseFieldNamesAreNotRenamed()
        {
        }
    }
}
