using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace XSSAttacksFilter
{
    public class HtmlFilter
    {
        #region 构造
        public HtmlFilter() : this(FilterPolicy.GetInstance()) { }
        public HtmlFilter(FileInfo FilterPolicyFile) : this(FilterPolicy.GetInstance(FilterPolicyFile)) { }
        public HtmlFilter(string FilterPolicyFilePath) : this(FilterPolicy.GetInstance(FilterPolicyFilePath)) { }
        public HtmlFilter(FilterPolicy policy)
        {
            if (policy == null)
            {
                throw new Exception();
            }
            Policy = policy;
        }
        #endregion

        #region 属性
        public FilterPolicy Policy { get; private set; }
        CssFilter _cssFilter;
        public CssFilter CssFilter
        {
            get
            {
                if (_cssFilter == null) { _cssFilter = new CssFilter(Policy); }
                return _cssFilter;
            }
        }
        #endregion

        #region 公共方法
        
        public virtual string Filters(string html)
        {
            if (html == null || html.Length == 0)
            {
                return string.Empty;
            }
            //had problems with the &nbsp; getting double encoded, so this converts it to a literal space.  
            //this may need to be changed.
            html = html.Replace("&nbsp;", char.Parse("\u00a0").ToString());
            //We have to replace any invalid XML characters
            html = StripNonValidXMLCharacters(html);

            int maxInputSize = Policy.Directive<int>("maxInputSize");

            //ensure our input is less than the max
            if (maxInputSize > 0 && maxInputSize < html.Length)
            {
                return string.Empty;
            }

            //修复一些敏感标签
            if (!HtmlNode.ElementsFlags.Contains("iframe"))
                HtmlNode.ElementsFlags.Add("iframe", HtmlElementFlag.Empty);
            HtmlNode.ElementsFlags.Remove("form");

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            //设置自动添加结束标签
            doc.OptionAutoCloseOnEnd = true;
            //设置强制执行XML规则
            doc.OptionOutputAsXml = true;
            FiltersTags(doc.DocumentNode.ChildNodes);
            return doc.DocumentNode.InnerHtml;
        }
        #endregion

        #region 帮助方法
        /// <summary>
        /// 过滤标签集合
        /// </summary>
        /// <param name="nodes"></param>
        void FiltersTags(HtmlNodeCollection nodes)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                HtmlNode tmp = nodes[i];
                FiltesTag(tmp);
                if (tmp.ParentNode == null)
                {
                    i--;
                }
            }
        }
        /// <summary>
        /// 过滤指定编辑器的属性和子元素
        /// </summary>
        /// <param name="node"></param>
        void FiltesTag(HtmlNode node)
        {
            string tagName = node.Name.ToLower();
            if (tagName.Equals("#text")) return;
            var tag = Policy.Tag(tagName);
            PolicyHtmlTagAction actoin = tag == null ? PolicyHtmlTagAction.Filter : tag.Action;
            switch (actoin)
            {
                case PolicyHtmlTagAction.Filter:
                    ///删除当前节点，但保留其有效的子节点
                    PromoteChildren(node);
                    return;
                case PolicyHtmlTagAction.Validate:
                    ///过滤当前元素的属性与及子节点
                    ValidateAction(node, tagName, tag);
                    return;
                case PolicyHtmlTagAction.Truncate:
                    ///删除当前节点的所有属性以及子节点，但保留文本和备注节点。
                    TruncateAction(node);
                    return;
                default:
                    ///将当前节点从父节点中删除。
                    HtmlNode parentNode = node.ParentNode;
                    parentNode.RemoveChild(node);
                    break;
            }

        }

        void ValidateAction(HtmlNode node,string tagName,PolicyHtmlTag tag)
        {
            HtmlNode parentNode = node.ParentNode;
            #region 过滤样式
            if ("style".Equals(tagName))
            {
                try
                {
                    node.FirstChild.InnerHtml = CssFilter.Filters(node.FirstChild.InnerHtml);
                }
                catch
                {
                    parentNode.RemoveChild(node);
                }
            }
            #endregion

            #region 过滤属性
            for (int currentAttributeIndex = 0; currentAttributeIndex < node.Attributes.Count; currentAttributeIndex++)
            {
                HtmlAttribute attribute = node.Attributes[currentAttributeIndex];
                string name = attribute.Name, _value = attribute.Value;
                var attr = tag.AllowedAttribute(name);

                #region 如果是白名单之外的属性移除掉
                if (attr == null)
                {
                    node.Attributes.Remove(name);
                    currentAttributeIndex--;
                    continue;
                }
                #endregion
                #region 元素内嵌样式
                if ("style".Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        attribute.Value = CssFilter.Filters(_value,true);
                    }
                    catch
                    {
                        node.Attributes.Remove(name);
                        currentAttributeIndex--;
                    }
                    continue;
                }
                #endregion
                ///如果未能通过验证，将执行指定的操作
                if (!FilterPolicy.ValidateAttribute(attr,_value))
                {
                    switch (attr.OnInvalid)
                    {
                        case PolicyHtmlAttributeOnInvalid.RemoveTag:
                            //删除当前的元素并退出函数
                            parentNode.RemoveChild(node);
                            return;
                        case PolicyHtmlAttributeOnInvalid.FilterTag:
                            ///删除当前节点，但保留其有效的子节点
                            PromoteChildren(node);
                            return;
                        default:
                            //删除当前的属性，指针往回调
                            node.Attributes.Remove(attr.Name);
                            currentAttributeIndex--;
                            break;
                    }

                }
            }
            #endregion
            ///过滤当前元素的子节点
            FiltersTags(node.ChildNodes);
    
        }
        void FilterAction(HtmlNode node)
        {
            FiltersTags(node.ChildNodes);
        }
        /// <summary>
        /// 删除所有的属性和子元素，但保留文本和备注节点
        /// </summary>
        /// <param name="node"></param>
        void TruncateAction(HtmlNode node)
        {
            HtmlAttributeCollection attrs = node.Attributes;
            while (attrs.Count > 0)
            {
                node.Attributes.Remove(attrs[0].Name);
            }
            HtmlNodeCollection nodes = node.ChildNodes;
            int position = 0;
            while (nodes.Count > position)
            {
                HtmlNode nodeToRemove = nodes[position];
                var type = nodeToRemove.NodeType;
                if (type == HtmlNodeType.Text || type == HtmlNodeType.Comment) { position++; continue; }
                node.RemoveChild(nodeToRemove);
            }
        }
        

        /// <summary>
        /// 去除无效的XML字符
        /// </summary>
        /// <param name="in_Renamed"></param>
        /// <returns></returns>
        string StripNonValidXMLCharacters(string in_Renamed)
        {
            StringBuilder out_Renamed = new StringBuilder();
            char current;
            if (in_Renamed == null || ("".Equals(in_Renamed)))
                return "";
            for (int i = 0; i < in_Renamed.Length; i++)
            {
                current = in_Renamed[i];
                if ((current == 0x9) || (current == 0xA) || (current == 0xD) || ((current >= 0x20) && (current <= 0xD7FF)) || ((current >= 0xE000) && (current <= 0xFFFD)))
                    out_Renamed.Append(current);
            }
            return out_Renamed.ToString();
        }
        /// <summary>
        /// 将指定节点从父节点中移除，但其子节点保留
        /// </summary>
        /// <param name="node"></param>
        void PromoteChildren(HtmlNode node)
        {
            ///过滤子节点
            FiltersTags(node.ChildNodes);
            HtmlNodeCollection nodeList = node.ChildNodes;
            HtmlNode parent = node.ParentNode;
            ///将它的所有子节点往上移到父节点的前面
            while (nodeList.Count > 0)
            {
                HtmlNode removeNode = node.RemoveChild(nodeList[0]);
                parent.InsertBefore(removeNode, node);
            }
            //然后将节点删除
            parent.RemoveChild(node);
        }
        #endregion

    }
}
