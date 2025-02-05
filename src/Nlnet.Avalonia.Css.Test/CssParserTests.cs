using System.Diagnostics;
using Avalonia.Styling;

namespace Nlnet.Avalonia.Css.Test
{
    [TestClass]
    public class CssParserTests
    {
        [TestMethod]
        public void RemoveCommentsTest()
        {
            IAcssBuilder builder = new AcssBuilder();
            var         parser  = builder.Parser;

            var s1 = parser.RemoveCommentsAndLineBreaks("/**/abc".ToCharArray());
            var s2 = parser.RemoveCommentsAndLineBreaks("a/*abc*/bc".ToCharArray());
            var s3 = parser.RemoveCommentsAndLineBreaks("abc/*abc*/".ToCharArray());
            var s4 = parser.RemoveCommentsAndLineBreaks("abc//*abc*/".ToCharArray());
            var s5 = parser.RemoveCommentsAndLineBreaks("abc/*a/bc*/".ToCharArray());
            var s6 = parser.RemoveCommentsAndLineBreaks("abc/*a/*bc*/".ToCharArray());
            var s7 = parser.RemoveCommentsAndLineBreaks("abc/*a*/bc*/-".ToCharArray());
            var s8 = parser.RemoveCommentsAndLineBreaks("abc/*abc**//-".ToCharArray());
            var s9 = parser.RemoveCommentsAndLineBreaks("ab/*c//asdf*/asd".ToCharArray());
            var s10 = parser.RemoveCommentsAndLineBreaks("a//bc\rdef".ToCharArray());
            var s11 = parser.RemoveCommentsAndLineBreaks("abc//d\nef".ToCharArray());
            var s12 = parser.RemoveCommentsAndLineBreaks("ab//c\r\ndef".ToCharArray());


            Assert.AreEqual(s1.ToString(), "abc");
            Assert.AreEqual(s2.ToString(), "abc");
            Assert.AreEqual(s3.ToString(), "abc");
            Assert.AreEqual(s4.ToString(), "abc");
            Assert.AreEqual(s5.ToString(), "abc");
            Assert.AreEqual(s6.ToString(), "abc");
            Assert.AreEqual(s7.ToString(), "abc-");
            Assert.AreEqual(s8.ToString(), "abc/-");
            Assert.AreEqual(s9.ToString(), "abasd");
            Assert.AreEqual(s10.ToString(), "adef");
            Assert.AreEqual(s11.ToString(), "abcef");
            Assert.AreEqual(s12.ToString(), "ab def");
        }

        [TestMethod]
        public void EfficientCssParserTest()
        {
            IAcssBuilder builder  = new AcssBuilder();

            var parser   = builder.Parser;
            var cssFile  = File.ReadAllText("./Assets/nlnet.blog.css");
            var sections = parser.ParseSections(null, cssFile);
            var styles   = sections.OfType<IAcssStyle>();

            foreach (var acssStyle in styles)
            {
                Trace.WriteLine(acssStyle.ToString());
            }
        }

        [TestMethod]
        public void TypeProviderTest()
        {
            IAcssBuilder builder  = new AcssBuilder();

            var acssFile  = File.ReadAllText("./Assets/avalonia.controls.css");
            var parser   = builder.Parser;
            var css      = parser.RemoveCommentsAndLineBreaks(new Span<char>(acssFile.ToCharArray()));
            var sections = parser.ParseSections(null, css);
            var styles   = sections.OfType<IAcssStyle>();

            foreach (var acssStyle in styles)
            {
                var style    = acssStyle.ToAvaloniaStyle();
                var selector = style!.Selector;
                Trace.WriteLine(selector != null ? selector.ToString() : "<selector is null.>");
            }
        }
    }
}