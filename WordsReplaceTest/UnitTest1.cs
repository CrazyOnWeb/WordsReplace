using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WordsReplace;

namespace WordsReplaceTest {
    [TestClass]
    public class UnitTest1 {
        [TestMethod]
        public void StrAvgSplit_SplitItems() {
            Equal(new string[] { 
            "1"}, "1".SplitItems(6));

            Equal(new string[] { 
            "12"}, "12".SplitItems(6));

            Equal(new string[] { 
            "123"}, "123".SplitItems(6));

            Equal(new string[] { 
            "1234"}, "1234".SplitItems(6));

            Equal(new string[] { 
            "12345"}, "12345".SplitItems(6));

            Equal(new string[] { 
            "123456"}, "123456".SplitItems(6));

            Equal(new string[] { 
            "123",
            "456",
            "7"}, "1234567".SplitItems(6));

            Equal(new string[] { 
            "1234",
            "5678"}, "12345678".SplitItems(6));

            Equal(new string[] { 
            "1234",
            "5678",
            "9"}, "123456789".SplitItems(6));

            Equal(new string[] { 
            "12345",
            "67890"}, "1234567890".SplitItems(6));

            Equal(new string[] { 
            "12345",
            "67890",
            "1"}, "12345678901".SplitItems(6));

            Equal(new string[] { 
            "123456",
            "789012"}, "123456789012".SplitItems(6));

            Equal(new string[] { 
            "1234",
            "5678",
            "9012","3"}, "1234567890123".SplitItems(6));



            Equal(new string[] { 
            "1"}, "1".SplitItems(3));

            Equal(new string[] { 
            "12"}, "12".SplitItems(3));

            Equal(new string[] { 
            "123"}, "123".SplitItems(3));

            Equal(new string[] { 
            "12","34"}, "1234".SplitItems(3));

            Equal(new string[] { 
            "12","34","5"}, "12345".SplitItems(3));

            Equal(new string[] { 
            "123","456"}, "123456".SplitItems(3));

            Equal(new string[] { 
            "12",
            "34",
            "56",
            "7"}, "1234567".SplitItems(3));

            Equal(new string[] { 
            "12",
            "34",
            "56","78"}, "12345678".SplitItems(3));

        }


        private void Equal(string[] expected, string[] actual) {
            Assert.AreEqual(expected.Length, actual.Length);
            for (int i = 0; i < expected.Length; i++) {
                Assert.AreEqual(expected[i], actual[i]);
            }
        }
    }
}
