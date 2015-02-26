using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Caching;
using System.Text.RegularExpressions;
using System.Web.Configuration;
using HtmlAgilityPack;

namespace XSSAttacksFilter
{
    [Serializable]
    public class FilterPolicy
    {
        Dictionary<string, string> commonRegularExpressions, directives;
        Dictionary<string, PolicyHtmlAttribute> commonAttributes,globalAttributes;
        Dictionary<string, PolicyHtmlTag> tagRules;
        Dictionary<string, PolicyCssProperty> cssRules;

        private FilterPolicy() {
            XDocument doc = LoadPolicyFile(Assembly.GetExecutingAssembly().GetManifestResourceStream("XSSAttacksFilter.resources.DefaultPolicy.xml"));
            Init(doc);
        }
        private FilterPolicy(string fileName)
        {
            XDocument doc;
            try
            {
                doc = LoadPolicyFile(fileName);
            }
            catch (Exception x) { throw new FilterPolicyException("无效的XSSAttacks过滤策略。", x); }
            try
            {
                Init(doc);
            }
            catch (Exception x) { throw new FilterPolicyException("XSSAttacks策略文档不是一个有效的架构。",x); }
        }
        XDocument LoadPolicyFile(Stream stream)
        {
          return XDocument.Load(stream);
        }
        XDocument LoadPolicyFile(string fileName) 
        {
            return XDocument.Load(fileName);
        
        }

        void Init(XDocument doc)
        {
            XElement root=doc.Root;
            var commonRegularExpressionListNode = root.Element("common-regexps");
            commonRegularExpressions = ParseNamesValues(commonRegularExpressionListNode.Elements("regexp"));

            var directiveListNode = root.Element("directives");
            this.directives = ParseNamesValues(directiveListNode.Elements("directive"));

            var commonAttributeListNode = root.Element("common-attributes");
            this.commonAttributes = ParseHtmlAttributes(commonAttributeListNode);

            var globalAttributesListNode = root.Element("global-tag-attributes");
            this.globalAttributes = ParseHtmlAttributes(globalAttributesListNode);

            var tagListNode = root.Element("tag-rules");
            this.tagRules = ParseHtmlTags(tagListNode);

            var cssListNode = root.Element("css-rules");
            this.cssRules = ParseCssProperties(cssListNode);

        }

        #region 静态方法
        /// <summary>
        /// 从高速缓存中获取安全策略信息，如果不存在将从文件中加载并保存到缓存中
        /// </summary>
        /// <param name="fileName">文件的物理绝对路径</param>
        /// <returns></returns>
        public static FilterPolicy GetInstance(string fileName = null)
        {
            return GetFilterPolicy(fileName);
        }
        public static FilterPolicy GetInstance(FileInfo file)
        {
            return file == null || !file.Exists ? null : GetFilterPolicy(file.FullName);
        }
        static FilterPolicy GetFilterPolicy(string fileName = null)
        {
            bool noFile = false;
            if (string.IsNullOrWhiteSpace(fileName))
            {
                ///读取配置的默认过滤策略，并将其转换为物理路径。
                fileName = WebConfigurationManager.AppSettings == null ? null : WebConfigurationManager.AppSettings["XSSAttacksFilter:DefaultPolicy"];
                if (!string.IsNullOrWhiteSpace(fileName))
                {
                    fileName = fileName.Trim().Replace("\\", "/");
                    if (!fileName.StartsWith("/") && !fileName.StartsWith("~/"))
                    { fileName = "/" + fileName; }
                    fileName = HttpContext.Current.Server.MapPath(fileName);
                }
            }

            string key;
            if (string.IsNullOrWhiteSpace(fileName))
            {
                key = "XSSAttacksFilter.resources.DefaultPolicy.xml"; noFile = true;
            }
            else
            {
                key = "XSSAttacksFilter:" + fileName;
            }
            var p = HttpContext.Current == null || HttpContext.Current.Cache == null ? null : HttpContext.Current.Cache[key] as FilterPolicy;
            if (p == null)
            {
                p = noFile ? new FilterPolicy() : new FilterPolicy(fileName);
                //是否要进行高速缓存
                string str = p.Directive("cacheTimeSpan");
                Match m;
                if (str != null && (m = Regex.Match(str, @"^\s*(\d+(\.\d+)?)([dhm])?\s*$", RegexOptions.IgnoreCase)).Success)
                {
                    string flag = m.Groups[3].Value.ToLower();
                    double time = Convert.ToDouble(m.Groups[1].Value);
                    TimeSpan span = flag == "d" ? TimeSpan.FromDays(time) : flag == "h" ? TimeSpan.FromHours(time) : TimeSpan.FromMinutes(time);
                    HttpContext.Current.Cache.Add(key, p
                        , noFile ? null : new CacheDependency(fileName)
                        , Cache.NoAbsoluteExpiration, span, CacheItemPriority.Normal, null);
                }
            }

            return p;

        }
        /// <summary>
        /// 验证属性的值是否有效
        /// </summary>
        /// <param name="attr"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool ValidateAttribute(PolicyAttribute attr, string value)
        {
            if (attr == null || string.IsNullOrWhiteSpace(value)) return false;
            value = HtmlEntity.DeEntitize(value.Trim());
            ////验证是否在限定的值之内
            if (attr.AllowedValues != null)
            {
                foreach (string allowedValue in attr.AllowedValues)
                {
                    if (allowedValue != null && allowedValue.Equals(value, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            if (attr.AllowedRegExp != null)
            {
                ///验证是否符合指定的正则表达式
                foreach (string ptn in attr.AllowedRegExp)
                {
                    string pattern = ptn;
                    if (!pattern.StartsWith("^")) { pattern = "^" + pattern; }
                    if (!pattern.EndsWith("$")) { pattern = pattern + "$"; }
                    if (Regex.IsMatch(value, pattern))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        #endregion

        #region 公共方法
        public PolicyHtmlTag Tag(string tagName)
        { 
            return tagName == null||tagRules==null|| !tagRules.ContainsKey(tagName = tagName.ToLower()) ? null:tagRules[tagName];
        }
        public PolicyCssProperty CssProperty(string name)
        {
            return name == null || cssRules == null || !cssRules.ContainsKey(name = name.ToLower()) ? null : cssRules[name];
        }
        public PolicyHtmlAttribute CommonHtmlAttribute(string name)
        {
            return name == null || commonAttributes == null || !commonAttributes.ContainsKey(name) ? null : commonAttributes[name];
        }
        public PolicyHtmlAttribute GlobalHtmlAttribute(string name)
        {
            return name == null || globalAttributes == null || !globalAttributes.ContainsKey(name) ? null : globalAttributes[name];
        }
        public string RegularExpression(string name)
        {
            return name == null || commonRegularExpressions == null || !commonRegularExpressions.ContainsKey(name = name.ToLower()) ? null : commonRegularExpressions[name];
        }
        public virtual string Directive(string name)
        {
            return name == null || directives == null || !directives.ContainsKey(name = name.ToLower()) ? null : directives[name];
        }
        public virtual T Directive<T>(string name)where T : struct
        {
            string v = Directive(name);
            if (string.IsNullOrWhiteSpace(v)) return default(T);
            Type t = typeof(T);
            try
            {
                if (t.IsEnum)
                {
                    return (T)Enum.Parse(t, v);
                }
                else
                {
                    return (T)Convert.ChangeType(v, t);
                }
            }
            catch
            {
                if (t == typeof(Guid))
                {
                    object o = new Guid(v);
                    return (T)o;
                }
            }
            return default(T);
        }
        #endregion

        #region 帮助方法

        string[] GetRegexList(XElement e)
        {
            char regexpBegin = '^', regexpEnd = '$';
            var regExpListNode = e.Element("regexp-list");
            IEnumerable<XElement> nodes;
            if (regExpListNode == null || (nodes = regExpListNode.Elements("regexp")) == null || nodes.Count() == 0)
            {
                return null;   
            }
            return (from n in nodes
                    let name = Attr(n, "name")
                    let vale = Attr(n, "value")
                    where (name != null && name.Length > 0) || (vale != null && vale.Length > 0)
                    select name == null || name.Length == 0 ? regexpBegin + vale + regexpEnd : RegularExpression(name)).ToArray();
        }
        string[] GetLiteralList(XElement e)
        {
            var regExpListNode = e.Element("literal-list");
            IEnumerable<XElement> nodes;
            if (regExpListNode == null || (nodes = regExpListNode.Elements("literal")) == null || nodes.Count() == 0)
            {
                return null;
            }
            return (from n in nodes
                    let vale = Attr(n, "value") ?? Value(n)
                    where (vale != null && vale.Length > 0)
                    select vale).ToArray();
        }
        Dictionary<string,string> ParseNamesValues(IEnumerable<XElement> elements)
        {
            if (elements == null) return null;
            var list = from e in elements select new { name = Attr(e, "name"), value = Attr(e, "value") };
            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (var n in list)
            {
                if (n!=null &&!string.IsNullOrWhiteSpace(n.name)&&!string.IsNullOrWhiteSpace(n.value))
                {
                    string key = n.name.ToLower();
                    if (!dic.ContainsKey(key))
                    { dic.Add(key, n.value); }

                }
            }
            return dic;
        }
        Dictionary<string, PolicyHtmlAttribute> ParseHtmlAttributes(XElement e) {

            IEnumerable<XElement> elements = e.Elements("attribute");
            if (elements == null) return null;
            Dictionary<string, PolicyHtmlAttribute> attrs = new Dictionary<string, PolicyHtmlAttribute>();
            Func<string, PolicyHtmlAttributeOnInvalid> ParseOnInvalid = s => { PolicyHtmlAttributeOnInvalid tmp; return s!=null && Enum.TryParse<PolicyHtmlAttributeOnInvalid>(s, out tmp) ? tmp : PolicyHtmlAttributeOnInvalid.RemoveAttribute; };
            foreach (var node in elements)
            {
                String name = Attr(node,"name");
                if (string.IsNullOrWhiteSpace(name)) continue;
                PolicyHtmlAttribute attr = new PolicyHtmlAttribute(this, name);
                attr.OnInvalid = ParseOnInvalid(Attr(node, "onInvalid"));
                attr.Description = Attr(node, "description");
                attr.AllowedRegExp = GetRegexList(node);
                attr.AllowedValues = GetLiteralList(node);
                string key = attr.Name.ToLower();
                if (!attrs.ContainsKey(key))
                { attrs.Add(key, attr); }
            }
            return attrs;
        }
        Dictionary<string, PolicyHtmlTag> ParseHtmlTags(XElement e)
        {
            IEnumerable<XElement> elements = e.Elements("tag");
            if (elements == null) return null;
            Dictionary<string, PolicyHtmlTag> tags = new Dictionary<string, PolicyHtmlTag>();
            Func<string, PolicyHtmlTagAction> ParseAction = s => {
                PolicyHtmlTagAction action;
                return Enum.TryParse<PolicyHtmlTagAction>(s, out action) ? action : PolicyHtmlTagAction.Remove;
            }; 
            foreach (var tagNode in elements)
            {
                string name = Attr(tagNode,"name"),key;
                if (string.IsNullOrWhiteSpace(name) || tags.ContainsKey(key = name.Trim().ToLower())) continue;
                tags.Add(key
                    , new PolicyHtmlTag(this, ParseHtmlAttributes(tagNode))
                    {
                        Name = name
                        ,
                        Action = ParseAction(Attr(tagNode, "action"))
                    });

            }
            return tags;
        
        }
        private Dictionary<string, PolicyCssProperty> ParseCssProperties(XElement e)
        {
            IEnumerable<XElement> elements = e.Elements("property");
            if (elements == null) return null;
            Dictionary<string, PolicyCssProperty> properties = new Dictionary<string, PolicyCssProperty>();

            foreach (var node in elements)
            {
                string name = Attr(node, "name"),key;
                if (string.IsNullOrWhiteSpace(name) || properties.ContainsKey(key = name.Trim().ToLower())) continue;
                PolicyCssProperty attr = new PolicyCssProperty(this, name);
                attr.Description = Attr(node, "description");
                attr.AllowedRegExp = GetRegexList(node);
                attr.AllowedValues = GetLiteralList(node);
                attr.Shorthands = node.Element("shorthand-list") == null ? null : (from n in node.Element("shorthand-list").Elements("shorthand")
                                                                                   let shn = Attr(n, "name")
                                                                                   where shn != null && shn.Length > 0
                                                                                   select shn).ToArray();
               
                properties.Add(key, attr);
            }

            return properties;
        }
        string Attr(XElement e, string name)
        {
            XAttribute attr = e == null || string.IsNullOrEmpty(name) ? null : e.Attribute(name);
            return attr == null ? null : HttpUtility.HtmlDecode(attr.Value);
        }
        string Value(XElement e)
        {
            return e == null ? null : e.Value;
        }
       
        #endregion

    }
}
