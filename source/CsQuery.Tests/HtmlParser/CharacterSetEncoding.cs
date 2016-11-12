﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;
using CollectionAssert = NUnit.Framework.CollectionAssert;
using Description = NUnit.Framework.DescriptionAttribute;
using TestContext = Microsoft.VisualStudio.TestTools.UnitTesting.TestContext;
using CsQuery;
using CsQuery.HtmlParser;
using CsQuery.Utility;

namespace CsQuery.Tests.HtmlParser
{

    [TestFixture, TestClass]
    public class CharacterSetEncoding : CsQueryTest
    {
        string htmlStart = @"<html><head>";
        string htmlStartMeta = @"<META HTTP-EQUIV='Content-Type' content='text/html;charset=windows-1255'>";
        string htmlStart3 = @"</head><body><div id=test>";
        string htmlEnd = "</div></body></html>";
        char hebrewChar = (char)164;

      

        [TestMethod, Test]
        public void MetaTag()
        {

            var encoder = Encoding.GetEncoding("windows-1255");

            var html = htmlStart + htmlStartMeta + htmlStart3 +hebrewChar + htmlEnd;
            var htmlNoRecode = htmlStart + htmlStart3 + hebrewChar + htmlEnd;


            // create a windows-1255 encoded stream
            
            var dom = CQ.Create(GetMemoryStream(htmlNoRecode, encoder));

            // grab the character from CsQuery's output, and ensure that this all worked out.
            
            var csqueryHebrewChar = dom["#test"].Text();

            // Test directly from the stream

            string htmlHebrew = new StreamReader(GetMemoryStream(htmlNoRecode,encoder),encoder).ReadToEnd();

            var sourceHebrewChar = htmlHebrew.Substring(htmlHebrew.IndexOf("test>") + 5, 1);

            // CsQuery should fail to parse it

            Assert.AreNotEqual(sourceHebrewChar, csqueryHebrewChar);


            // the actual character from codepage 1255
            Assert.AreEqual("₪", sourceHebrewChar);

            // Now try it same as the original test - but with the meta tag identifying character set.

            var htmlWindows1255 = GetMemoryStream(html, encoder);

            // Now run again with the charset meta tag, but no encoding specified.
            dom = CQ.Create(htmlWindows1255);

            csqueryHebrewChar = dom["#test"].Text();

            Assert.AreEqual(sourceHebrewChar,csqueryHebrewChar);
        }


        [TestMethod, Test]
        public void MetaTagOutsideBlock()
        {

            var encoder = Encoding.GetEncoding("windows-1255");

            string filler = "<script type=\"text/javascript\" src=\"dummy\"></script>";
            var html = htmlStart;
            
            for (int i = 1; i < 5000 / filler.Length; i++)
            {
                html += filler;
            }
            html += htmlStartMeta + htmlStart3;

            // pad enough after the meta so that the hebrew character is in block 3.
            // If it were in block 2, it could only reflect the prior encoding.
            
            for (int i = 1; i < 5000 / filler.Length; i++)
            {
                html += filler;
            }
            
            html+=hebrewChar + htmlEnd;

            // Now try it  same as the original test - but with the meta tag identifying character set.

            var htmlWindows1255 = GetMemoryStream(html, encoder);

            var dom = CQ.Create(htmlWindows1255, null);
            var outputHebrewChar = dom["#test"].Text();

            Assert.AreEqual("₪", outputHebrewChar);

        }

        private string arabicExpected = @"البابا: اوقفوا ""المجزرة"" في سوريا قبل ان تتحول البلاد الى ""أطلال""";
        
        
        /// <summary>
        /// Removes the "meta http-equiv='Content-Type'" header, or replaces it with a different character set
        /// </summary>
        ///
        /// <param name="html">
        /// The HTML.
        /// </param>
        ///
        /// <returns>
        /// Cleaner HTML
        /// </returns>

        private string ReplaceCharacterSet(string html, string with="")
        {
            // remove the content type header
            var start = html.IndexOf(@"<meta http-equiv=""Content-Type""");
            var end = html.IndexOf(">", start);

            string replaceWith = "";
            if (!String.IsNullOrEmpty(with))
            {
                replaceWith = String.Format(@"<meta http-equiv=""Content-Type"" content=""text/html; charset={0}"" />", with);
            }
            return html.Substring(0, start) + replaceWith + html.Substring(end + 1);
        }

    }
}
