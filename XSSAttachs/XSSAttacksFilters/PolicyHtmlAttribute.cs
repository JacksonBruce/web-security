using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XSSAttacksFilter
{
    [Serializable]
    public class PolicyHtmlAttribute : PolicyAttribute
    {
        public PolicyHtmlAttribute(FilterPolicy policy, string name, PolicyHtmlTag tag = null)
            : base(policy,name)
        {
            Tag = tag;
        }
        public PolicyHtmlTag Tag { get;internal set; }
        public PolicyHtmlAttributeOnInvalid OnInvalid
        {
            get;
            set;
        }
      
    }
}
