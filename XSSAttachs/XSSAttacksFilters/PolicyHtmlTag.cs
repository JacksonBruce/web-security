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
        public PolicyHtmlAttribute AllowedAttribute(string name)
        {
            var a = allowedAttributes.ContainsKey(name) ? allowedAttributes[name] : null;
            if (a == null)
            {
                a = Policy.GlobalHtmlAttribute(name);
            }
            return a;
        }
        

    }
}
