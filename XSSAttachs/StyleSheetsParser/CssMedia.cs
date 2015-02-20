using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StyleSheetsParser
{
    public class CssMedia : CssRule, ICssRulesContainer
    {
        public CssMedia(string name, IList<CssRule> selectors)
            : base(name) 
        {
            this.Selectors = selectors;
        }
        bool? _isValid;
        public override bool IsValid
        {
            get
            {
                if (!_isValid.HasValue)
                {
                    //媒体必须是符合下面的规则，否则视为为无效的
                    _isValid = Regex.IsMatch(Name, @"^@media\s+", RegexOptions.IgnoreCase);

                }
                return _isValid.Value;
            }
            set { _isValid = value; }
        }
        public IEnumerable<CssRule> Selectors { get; private set; }
        protected override string GetCssString()
        {
            if (Selectors == null || Selectors.Count() == 0) return string.Empty;
            StringBuilder sb = new StringBuilder(Name + "{");
            foreach (var s in Selectors)
            {
                //sb.AppendLine();
                sb.Append(s);
            }
            sb.Append("}");
            return sb.ToString();
        }

     
    }
}
