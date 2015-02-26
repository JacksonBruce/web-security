using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StyleSheetsParser
{
    public class CssAttribute:CssRule
    {
        public CssAttribute(string name, string value = null)
            : base(name)
        { if (value != null) { this.Value = Regex.Replace(value.Trim().Replace("\n", " ").Replace("\r", ""), @"\s{2}", " "); } }
        public string Value { get;set; }
        protected override string GetCssString()
        {
            return string.IsNullOrWhiteSpace(Value) ? string.Empty : string.Format("{0}:{1}", Name, Value);
        }
        bool? _isValid;
        public override bool IsValid { get {
            if (!_isValid.HasValue)
            {
                //属性必须是以下字符开始，否则视为为无效的
                _isValid = Regex.IsMatch(Name, @"^[\*_\-a-z]", RegexOptions.IgnoreCase);

            }
            return _isValid.Value; } set { _isValid = value; } }

        //public static CssAttribute Parse(string source)
        //{
        //    string[] arr;
        //    return source == null || 2 != (arr = source.Split(':')).Length || (arr[0] = (arr[0] ?? "").Trim()) == "" || (arr[1] = (arr[1] ?? "").Trim()) == "" ?
        //        null : new CssAttribute(arr[0], arr[1]);

        //}
        //public static IList<CssAttribute> ParseAttributes(string source)
        //{
        //    if (source == null || (source = source.Trim('{', '}', ' ', ';')) == "") return null;
        //    string[] arr = source.Split(';');
        //    List<CssAttribute> list = new List<CssAttribute>();
        //    foreach (var s in arr)
        //    {
        //        var attr = Parse(s);
        //        if (attr == null) continue;
        //        list.Add(attr);
        //    }
        //    return list;
        
        //}
    }
}
