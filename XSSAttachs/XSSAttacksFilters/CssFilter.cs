using HtmlAgilityPack;
using StyleSheetsParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace XSSAttacksFilter
{
    public class CssFilter
    {

        #region 构造
        public CssFilter() : this(FilterPolicy.GetInstance()) { }
        public CssFilter(FileInfo FilterPolicyFile) : this(FilterPolicy.GetInstance(FilterPolicyFile)) { }
        public CssFilter(string FilterPolicyFilePath) : this(FilterPolicy.GetInstance(FilterPolicyFilePath)) { }
        public CssFilter(FilterPolicy policy)
        {
            if (policy == null)
            {
                throw new Exception();
            }
            Policy = policy;
            EmbedStyleSheets = policy.Directive<bool>("embedStyleSheets");
        }
        #endregion
       
        public FilterPolicy Policy { get; private set; }
        public bool EmbedStyleSheets { get; private set; }
        bool Validate(CssAttribute attr)
        {
            return attr != null && FilterPolicy.ValidateAttribute(Policy.CssProperty(attr.Name), attr.Value);
        }
        void Validate(IEnumerable<CssRule> rules)
        {
            if (rules == null || rules.Count() == 0) return;
            foreach (var rule in rules)
            {
                CssAttribute attr = rule as CssAttribute;
                if (attr != null)
                {
                    if (attr.IsValid) attr.IsValid = Validate(attr);
                    continue;
                }
                CssImport import = rule as CssImport;
                if (import != null)
                {
                    import.IsValid = EmbedStyleSheets;
                }
                ICssRulesContainer container = rule as ICssRulesContainer;
                if (container != null)
                {
                    Validate(container.Selectors);
                }
            }
        }
        CssDocument Validate(string source)
        {
            CssDocument doc = CssDocument.Load(source);
            if (doc != null && doc.Rules != null && doc.Rules.Count > 0)
            {
                Validate(doc.Rules);
            }
            return doc;
        }
        public string Filters(string code, bool embedded=false)
        {
            string s = null;
            if (embedded)
            {
                s = ".Embedded{";
                code = s + code + "}";
            }
            CssDocument css;
            return code == null || code.Length == 0 || (css = Validate(code)) == null
                ? string.Empty : embedded ? Regex.Replace(css.ToString(), "^" + Regex.Escape(s), "").TrimEnd('}') : css.ToString();
        }
    }
}
