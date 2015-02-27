using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XSSAttacksFilter
{
    [Serializable]
    public class PolicyHtmlTag
    {
        public PolicyHtmlTag(FilterPolicy policy, Dictionary<string, PolicyHtmlAttribute> attributes)
        {
            Policy = policy;
            if (attributes != null)
            {
                foreach (var a in attributes) {
                    if (a.Value != null) { a.Value.Tag = this; }
                }
                this.allowedAttributes = attributes;
            }
        }
        public FilterPolicy Policy { get; private set; }
        private Dictionary<string, PolicyHtmlAttribute> allowedAttributes;
        public PolicyHtmlTagAction Action
        {
            get;
            set;
        }
        public string Name
        {
            get;
            set;
        }
        string[] MergerArray(string[] First, string[] Second) {

            string[] arr = new string[First.Length + Second.Length];
            First.CopyTo(arr, 0);
            Second.CopyTo(arr, First.Length);
            return arr.Distinct().ToArray();
        }
        void MergerRules(PolicyHtmlAttribute a, string[] AllowedRegExp, string[] AllowedValues)
        {
            if (a == null) return;
            if (AllowedRegExp != null)
            {
                if (a.AllowedRegExp == null) { a.AllowedRegExp = AllowedRegExp; }
                else { a.AllowedRegExp = MergerArray(a.AllowedRegExp, AllowedRegExp); }
            }
            if (AllowedValues != null)
            {
                if (a.AllowedValues == null) { a.AllowedRegExp = AllowedRegExp; }
                else { a.AllowedRegExp = MergerArray(a.AllowedRegExp, AllowedRegExp); }
            }
        }
        public PolicyHtmlAttribute AllowedAttribute(string name)
        {
            PolicyHtmlAttribute a = allowedAttributes.ContainsKey(name) ? allowedAttributes[name] : null,g=Policy.GlobalHtmlAttribute(name),c=Policy.CommonHtmlAttribute(name);
            if (a == null){a = g;}
            else if(g!=null){MergerRules(a, g.AllowedRegExp, g.AllowedValues);}
            if (a != null&&c!=null)
            {MergerRules(a, c.AllowedRegExp, c.AllowedValues);}
            return a;
        }
        

    }
}
