// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClassLibraryTests.cs" company="Microsoft">
//   Microsoft
// </copyright>
// <summary>
//   Defines the ClassLibraryTests type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Microsoft.EngSys.Samples
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// The class library tests.
    /// </summary>
    [TestClass]
    public class ClassLibraryTests
    {
        /// <summary>
        /// Testing GetIntegerSuccess
        /// </summary>
        [TestMethod]
        public void GetIntegerSuccess()
        {
            // Arrange
            int expectedValue = 2;

            var myTestClass = new Simple();

            // Assert
            Assert.AreEqual(myTestClass.GetInteger(), expectedValue);
        }

        /// <summary>
        /// Testing GetIntegerSuccess
        /// </summary>
        [TestMethod]
        public void GetIntegerFail()
        {
            // Arrange
            int expectedValue = 1;

            var myTestClass = new Simple();

            // Assert
            Assert.AreNotEqual(myTestClass.GetInteger(), expectedValue);
        }
    }
}
