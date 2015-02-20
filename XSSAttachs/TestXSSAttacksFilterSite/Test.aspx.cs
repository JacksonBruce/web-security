//using org.owasp.validator.html;
using StyleSheetsParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using XSSAttacksFilter;

namespace TestXSSAttacksFilterSite
{
    public partial class Test : System.Web.UI.Page
    {
        //HtmlFilter antisamy;
        //Policy policy = null;
        //string filename = @"/resources/antisamy.xml";
        public StringBuilder html;
        protected void Page_Load(object sender, EventArgs e)
        {
         
        }
        void FilterAttacks(string str, Func<string, bool> fn=null,[CallerMemberName] string propertyName = null)
        {
            html.Append("\n== in == "+propertyName+" ==================================================\n原文:\n" + str + "\n");
            //html.Append("====================================================================================================");
            html.Append("JavaScript：\n" + ((RichText)str).JavascriptEncode);
            html.Append("\n过滤:\n" + ((RichText)str));
            html.Append((fn == null ? null : "\n状态：" + (fn(str) ? "成功！" : "失败")));
        }
        protected void btn_Click(object sender, EventArgs e)
        {
            html = new StringBuilder();
            FilterAttacks(txt.Text);
        }
    }
}