using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StyleSheetsParser
{
    public class CssSelector:CssRule,ICssRulesContainer
    {
        public CssSelector(string name, IList<CssAttribute> attributes)
            : base(name)
        {
            Attributes = attributes;
        }
        public IList<CssAttribute> Attributes { get; private set; }
        protected override string GetCssString()
        {
            if (Attributes == null || Attributes.Count == 0) return string.Empty;
            StringBuilder sb = new StringBuilder();
            foreach (var attr in Attributes)
            {
                string s = attr != null ? attr.ToString() : null;
                if (s != "" && s != null && s != string.Empty)
                {
                    sb.AppendFormat("{0}{1}", sb.Length > 0 ? ";" : null, attr);
                }
            }
            if (sb.Length > 0)
            {
                sb.Insert(0, Name + "{");
                sb.Append("}");
            }
            return sb.ToString();
        }
        bool? _isValid;
        public override bool IsValid { 
            get {
                if (!_isValid.HasValue)
                {
                    //选择器必须是以下字符开始，否则视为为无效的
                    _isValid = Regex.IsMatch(Name, @"^[\*\[#\.a-z]", RegexOptions.IgnoreCase);
                
                }


            return _isValid.Value; } 
            set { _isValid = value; } }

        public IEnumerable<CssRule> Selectors
        {
            get { return Attributes; }
        }
    }
}
