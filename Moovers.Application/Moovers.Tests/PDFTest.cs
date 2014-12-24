using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Moovers.Tests
{
    [TestClass]
    public class PdfTester
    {
        /// <summary>
        /// Ensure that we have the correct installation to generate PDFs
        /// </summary>
        [TestMethod]
        public void PdfTest()
        {
            var testDoc = @"<!DOCTYPE html><html><body><h1>TEST DOCUMENT</h1></body></html>";
            try 
            {
                var pdf = Business.Utility.General.GeneratePdf(testDoc, System.Drawing.Printing.PaperKind.Letter);
                Assert.IsNotNull(pdf, "Failed to generate pdf");
                Assert.IsTrue(pdf.Length > 0);
            }
            catch (Exception)
            {
                Assert.Fail("Failed to generate pdf -- exception thrown");
            }
        }

        /// <summary>
        /// Ensure we have a read/writable directory that we can store files
        /// </summary>
        [TestMethod]
        public void FileTest()
        {
            var file = new Business.Models.File {
                FileID = Guid.NewGuid(),
                SavedContent = new byte[10]
            };

            Assert.IsTrue(file.SavedContent.Length == 10, "Ensure we can read data from a pdf");

            // remove file from path
            File.Delete(file.GetPath());
        }
    }
}
