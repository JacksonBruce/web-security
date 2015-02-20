using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace TestWebsite
{
    public partial class HEX : System.Web.UI.Page
    {
        protected StringBuilder html;
        protected void Page_Load(object sender, EventArgs e)
        {
            html = new StringBuilder("<img src=");
            string str = "#";
            foreach (char ch in str)
            {
                html.Append(ch > 255 ? ch.ToString() : "&#"+((byte)ch).ToString("d7"));
            }
            html.Append(" onerror=");
            str = "alert(/中文/)";

            foreach (char ch in str)
            {
                html.Append("&#" + ((int)ch).ToString("d7"));
            }

            html.Append(" />");
            //html.Append(" />\n<br /><IMG SRC=&#0000106&#0000097&#0000118&#0000097&#0000115&#0000099&#0000114&#0000105&#0000112&#0000116&#0000058&#0000097&#0000108&#0000101&#0000114&#0000116&#0000040&#0000039&#0000088&#0000083&#0000083&#0000039&#0000041>");

        }
    }
}