using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
namespace XSSAttacksFilter
{
    public class RichText
    {
        #region 构造
        /// <summary>
        /// 实例化一个富文本对象
        /// </summary>
        /// <param name="text">未被过滤的源文本</param>
        /// <param name="FilterPolicyFile">过滤的安全策略文件信息对象</param>
        public RichText(string text,FileInfo FilterPolicyFile) : this(text, FilterPolicy.GetInstance(FilterPolicyFile)) { }
        /// <summary>
        /// 实例化一个富文本对象
        /// </summary>
        /// <param name="text">未被过滤的源文本</param>
        /// <param name="FilterPolicyFilePath">过滤的安全策略文件的物理路径</param>
        public RichText(string text, string FilterPolicyFilePath) : this(text, FilterPolicy.GetInstance(FilterPolicyFilePath)) { }
        /// <summary>
        /// 实例化一个富文本对象
        /// </summary>
        /// <param name="text">未被过滤的源文本</param>
        /// <param name="policy">过滤的安全策略，如果不提供将启用默认的安全策略</param>
        public RichText(string text, FilterPolicy policy = null)
        {
            this.text = text;
            this.policy = policy;
        }
        #endregion

        protected virtual void Init()
        {
            attributeEncode = cssEncode = eventAttributeEncode = html = htmlEncode = javascriptEncode = urlEncode = null;
        }
        string text;
        FilterPolicy policy;
        public FilterPolicy Policy { get { if (policy == null) { policy = FilterPolicy.GetInstance(); } return policy; } }
        string html;
        public string Html
        {
            get
            {
                if (html == null)
                {
                    html = new HtmlFilter(Policy).Filters(text);
                }
                return html;
            }
        }
        string htmlEncode;
        public string HtmlEncode
        {
            get
            {
                if (htmlEncode == null)
                {
                    htmlEncode = text==null?string.Empty: HttpUtility.HtmlEncode(text);
                }
                return htmlEncode;
            }
        }
        string attributeEncode;
        public string AttributeEncode
        {
            get
            {
                if (attributeEncode == null)
                {
                    attributeEncode = text == null ? string.Empty : HttpUtility.HtmlAttributeEncode(text);
                }
                return attributeEncode;
            }
        }
        string javascriptEncode;
        public string JavascriptEncode
        {
            get
            {
                if (javascriptEncode == null)
                {
                    Func<string, string> fn = s => {
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < s.Length; i++) {
                            var ch = s[i];
                            switch (ch)
                            { 
                                case'\'':
                                case'"':
                                case'\\':
                                case'\n':
                                case '\r':
                                    sb.Append(@"\x"+((int)ch).ToString("X"));
                                    break;
                                default:
                                    sb.Append(ch);
                                    break;

                            }
                        }
                        return sb.ToString();
                    };
                    javascriptEncode = text == null || text == string.Empty ? string.Empty
                        : fn(text);
                        //: Regex.Replace(Html, @"[\'""\n\r\t]", new MatchEvaluator(s => s.Value == null || s.Value.Length == 0 ? s.Value : @"\x" + ((int)s.Value[0]).ToString("X")));
                        //: Html.Trim().Replace(@"\", @"\x5C").Replace("'", @"\x27").Replace("\"", @"\x22").Replace("\n", "\\n").Replace("\r", "\\r");
                }
                return javascriptEncode;

            }
        }
        string urlEncode;
        public string UrlEncode
        {
            get
            {
                if (urlEncode == null)
                {
                    urlEncode = text == null ? string.Empty : HttpUtility.UrlEncode(text);
                }
                return urlEncode;
            }
        }
        string cssEncode;
        public string CssEncode
        {
            get
            {
                if (cssEncode == null)
                {
                    cssEncode = new CssFilter(Policy).Filters(text);
                }
                return cssEncode;
            }
        }
        string eventAttributeEncode;
        public string EventAttributeEncode
        {
            get
            {
                if (eventAttributeEncode == null)
                {
                    eventAttributeEncode = HttpUtility.HtmlAttributeEncode(JavascriptEncode);
                }
                return eventAttributeEncode;
            }
        }
        public override string ToString()
        {
            return Html;
        }

        #region 重载操作符
        /// <summary>
        /// 字符串隐式转换为富文本对象
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static implicit operator RichText(string text)
        {
            return new RichText(text);
        }
        /// <summary>
        /// 富文本对象隐式转换为字符串
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static implicit operator string(RichText text)
        {
            return text == null ? null : text.ToString();
        }
        /// <summary>
        /// 重载富文本对象与字符串相加，并返回富文本对象
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static RichText operator +(RichText a,string b)
        {
            a.text += b;
            a.Init();
            return a;
        }
        /// <summary>
        /// 重载字符串与富文本对象相加，并返回富文本对象
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static RichText operator +(string a, RichText b)
        {
            b.text = a + b.text;
            b.Init();
            return b;
        }
        /// <summary>
        /// 重载富文本对象与富文本对象相加，并返回富文本对象
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static RichText operator +(RichText a, RichText b)
        {
            a.text += b.text;
            a.Init();
            return a;
        }
        ///这个是强制转换操作符的重载，在这里不需要了，因为有隐式转换了
        //public static explicit operator string(RichText text)
        //{
        //    return text == null ? null : text.ToString();
        //}
        #endregion
    }
}
