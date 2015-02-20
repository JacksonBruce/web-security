using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
namespace StyleSheetsParser
{
    public class CssImport:CssRule
    {
        public CssImport(string source) : base("@import") {
            if (string.IsNullOrWhiteSpace(source)) return;
            string s = "(" + Regex.Escape("@import") + "\\s+)?";
            var m = Regex.Match(source.Trim(), s + @"url\(([^\)]+)\)([\w\W]+)?", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            if (!m.Success)
            {
                m = Regex.Match(source.Trim(), s + @"['""]([^'""]+)['""]([\w\W]+)?", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            }
            if (m.Success) {
                Uri = m.Groups[2].Value.Trim('\'', '"', ' ');
                Media = Regex.Replace(m.Groups[3].Value.Replace("\n", " "), @"\s{2}", " ").Trim(';', ' ');
            }
        }
        public string Uri { get;private set; }
        public string Media { get; private set; }

        bool? _isValid = true;
        public override bool IsValid { get { return _isValid.Value; } set { _isValid = value; } }
        protected override string GetCssString()
        {
            return Uri == null ? string.Empty : string.Format("{0} url('{1}'){2}{3};", Name, Uri, Media == null || Media == "" ? null : " ", Media);
        }
    }
}
