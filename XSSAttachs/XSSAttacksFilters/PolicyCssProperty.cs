using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XSSAttacksFilter
{
    [Serializable]
    public class PolicyCssProperty : PolicyAttribute
    {
        public PolicyCssProperty(FilterPolicy policy, string name) : base(policy, name) { }
        public string[] Shorthands { get; set; }
    }
}
