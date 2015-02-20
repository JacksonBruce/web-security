using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StyleSheetsParser
{
    public abstract class CssRule
    {
        public CssRule(string name) {
            if (string.IsNullOrWhiteSpace(name))
            { throw new CssException("样式规则名称不能为空。"); }
            this.Name = Regex.Replace(name.Trim().Replace("\n", " "), @"\s{2}", " ");
        }
        public string Name { get; private set; }
        public abstract bool IsValid{get;set;}
        protected abstract string GetCssString();
        public override string ToString()
        {
            return IsValid ? GetCssString() : string.Empty;
        }
    }
}
