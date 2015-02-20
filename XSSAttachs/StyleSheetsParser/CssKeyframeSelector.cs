using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace StyleSheetsParser
{
    public class CssKeyframeSelector:CssSelector
    {
        public CssKeyframeSelector(string name, IList<CssAttribute> attrs) : base(name, attrs) { }
        bool? _percent;
        public bool IsPercent
        {
            get
            {
                if (!_percent.HasValue)
                {
                    _percent = Regex.IsMatch(Name, @"^\d+[%]$");
                }
                return _percent.Value;
            }
        }
        bool? _isValid;
        public override bool IsValid
        {
            get
            {
                if (!_isValid.HasValue)
                {
                    ///选择器必须符合如下的名称否则视为无效的
                    Regex rgx = new Regex(@"^from|to|\d+[%]$");
                    _isValid = rgx.IsMatch(Name);
                }
                return _isValid.Value;
            }
            set { _isValid = value; }
        }
       
    }
}
