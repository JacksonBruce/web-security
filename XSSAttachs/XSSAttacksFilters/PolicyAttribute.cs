using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XSSAttacksFilter
{
    [Serializable]
    public class PolicyAttribute
    {
        public PolicyAttribute(FilterPolicy policy, string name)
        {
            Name = name; 
            Policy = policy;
        }
        public FilterPolicy Policy { get; private set; }
        public string[] AllowedRegExp
        {
            get;
            set;
        }
        public string[] AllowedValues
        {
            get;
            set;
        }
        public string Name
        {
            get;
            protected set;
        }
        public string Description
        {
            get;
            set;
        }
      
        
    }
}
