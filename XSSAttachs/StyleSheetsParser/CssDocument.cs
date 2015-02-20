using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StyleSheetsParser
{
    public partial class CssDocument:ICssRulesContainer
    {
        public CssDocument()
            : this(new List<CssRule>())
        {

        }
        public CssDocument(IList<CssRule> Rules, string Encoding = null)
        {
            this.Rules = Rules;
            this.Encoding = Encoding;
        }
        public string Encoding { get; protected set; }
        //public IList<CssImport> Imports { get; protected set; }
        //public IList<CssKeyframes> KeyframesList { get; protected set; }
        public IList<CssRule> Rules { get; protected set; }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(string.IsNullOrWhiteSpace(Encoding) ? string.Empty : "@charset \"" + (Encoding??"").Replace("\"","") + "\";");
            if (Rules != null)
            {
                foreach (var r in Rules) {
                    if (r==null && !r.IsValid) continue;
                    //sb.AppendLine(); 
                    sb.Append(r);
                }
            }
            return sb.ToString();
        }

        IEnumerable<CssRule> ICssRulesContainer.Selectors
        {
            get { return Rules; }
        }
    }
}
