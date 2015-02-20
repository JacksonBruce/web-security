using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StyleSheetsParser
{

    public class CssKeyframes : CssRule, ICssRulesContainer
    {
        public CssKeyframes(string name, IList<CssKeyframeSelector> selectors) : base(name) {
            _selectors = new List<CssKeyframeSelector>();
            if (selectors != null)
            {
                foreach (var s in selectors)
                {
                    if (!s.IsValid|| (_percent.HasValue && s.IsPercent != _percent.Value)) { continue; }
                    if (!_percent.HasValue) { _percent = s.IsPercent; }
                    _selectors.Add(s);
                }
            }
        }
        bool? _percent;
        public bool IsPercent
        {
            get
            {
                return _percent.Value;
            }
        }
        IList<CssKeyframeSelector> _selectors;
        public IList<CssKeyframeSelector> Selectors { get { return _selectors; } }
        bool? _isValid;
        public override bool IsValid { get {
            if (!_isValid.HasValue)
            {
                //关键帧必须是符合下面的规则，否则视为为无效的
                _isValid = Regex.IsMatch(Name, @"^@[\-a-z]*keyframes\s+", RegexOptions.IgnoreCase);

            }
            return _isValid.Value; } set { _isValid = value; } }
        protected override string GetCssString() {
            if (Selectors == null || Selectors.Count == 0) return string.Empty;
            StringBuilder sb = new StringBuilder( Name + "{");
            foreach (var s in _selectors)
            {
                sb.Append(s);
            }
            sb.Append("}");
            return sb.ToString();
        }
        //public static CssKeyframes Parse(string source)
        //{
        //    int i;
        //    if (string.IsNullOrWhiteSpace(source)
        //        || 1 > (i = (source = source.Trim()).IndexOf('{')) || i >= source.Length)
        //        return null;

        //    List<CssKeyframeSelector> selectors = new List<CssKeyframeSelector>();
        //    string name = source.Substring(0, i);
        //    string value = source.Substring(i).Trim();
        //    value = value.Substring(1, value.Length - 1).Trim();
        //    i = 0;
        //    while (i < value.Length)
        //    {
        //        int l= value.IndexOf('}', i);
        //        if (l <= 0) break;
        //        string s = value.Substring(i,l);
        //        selectors.Add(CssSelector.Parse(s, (n, attrs) => new CssKeyframeSelector(n, attrs)));
        //        i++;
        //    }
        //    return new CssKeyframes(name, selectors);
        //}



        IEnumerable<CssRule> ICssRulesContainer.Selectors
        {
            get { return Selectors; }
        }
    }
}
