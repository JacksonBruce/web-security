using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StyleSheetsParser
{
    public class CssParser
    {
        char[] Source;//, startChars = new char[] {'@','#','.'};
        long position;
        bool over = false;
        string encoding;
        public CssParser(string source)
        {
            position = 0;
            if (source != null)
            {
                string s = Regex.Replace(source, @"\/\*[\w\W]*?\*\/", "", RegexOptions.Multiline | RegexOptions.Compiled);
                Source = s.ToCharArray();
            }
        }
        char Read(bool move=true)
        {
            if (position >= Source.Length) { 
                 over = true;
                return '\0';
            }
            return Source[move ? position++ : position];
        }
        void Next()
        {
            if (over) return;
            position++;
            if (position >= Source.Length)
            {
                over = true;
            }
           
        }
        char GetRuleNameStartChar()
        {
            char ch = Read() ;
            if (over || ch == '@' || ch == '[' || ch == '*' || ch == '#' || ch == '.' || (ch >= '0' && ch <= '9') || (ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z'))
            {
                return ch;
            } 
            return GetRuleNameStartChar();
        }
        char ReadMainStartChar(string ruleName)
        {
            if (over) return '\0';
            char ch = Read();
            if ("@charset".Equals(ruleName, StringComparison.CurrentCultureIgnoreCase))
            {
                if (ch != '\'' && ch != '"') return ReadMainStartChar(ruleName);
                else { return ch; }
            }
            else if ("@import".Equals(ruleName, StringComparison.CurrentCultureIgnoreCase))
            {
                if (ch != ' ') return ReadMainStartChar(ruleName);
                else { return ch; }
            }
            if (ch != '{') return ReadMainStartChar(ruleName);
            else { return ch; }
        }
        char GetRuleMainEndChar(string ruleName,char start)
        {
            return "@charset".Equals(ruleName, StringComparison.CurrentCultureIgnoreCase) ? start :
                "@import".Equals(ruleName, StringComparison.CurrentCultureIgnoreCase) ? ';' : start == '{' ? '}' : '\0';
        }
        string NextRuleName() {
            if (over) return string.Empty;
            char start = GetRuleNameStartChar();
            if (over) return string.Empty;
            StringBuilder sb = new StringBuilder(start.ToString());
            char ch, end=' '; char[] ends = start == '@' ? new char[] { ' ', '\n', '\r', '\t', '{' } : new char[] { '{' };
            Action action = () =>
            {
                while ((ch = Read(false)) != '\0' && !ends.Any(o => (end=o) == ch))
                {
                    sb.Append(ch);
                    Next();
                }
            };
            action();
            if (over) return string.Empty;
            string s = sb.ToString().Trim();
            if ( start == '@' && end!='{' && !("@import".Equals(s, StringComparison.CurrentCultureIgnoreCase) || "@charset".Equals(s, StringComparison.CurrentCultureIgnoreCase)))
            {
                sb.Clear();
                sb.Append(s+end);
                ends = new char[] { '{' };
                ch = Read();
                action();
                return sb.ToString();
            }

            return s;


        }
        void RemoveSpace() {
            char ch = Read(false);
            while ((ch == '\n' || ch == '\r' || ch == '\t' || ch == ' ') && !over)
            {
                Next();
                ch = Read(false);
            }
        }
        CssRule NextRule(Func<string,CssRule>fn=null)
        {
            CssRule rule;
            string name = NextRuleName();
            if (name == string.Empty) return null;
            char ch, start = ReadMainStartChar(name), end = GetRuleMainEndChar(name, start);
            if (name.StartsWith("@media", StringComparison.CurrentCultureIgnoreCase))
            {
                List<CssRule> slrs = new List<CssRule>(); 
               while (!over)
               {
                   rule = NextRule();
                   if (rule != null)
                   { slrs.Add(rule); }
                   RemoveSpace();
                   if (Read(false) == end || over) { Next(); break; }
                  
               }
               return new CssMedia(name, slrs);
            }
            if (name.StartsWith("@") && name.IndexOf("keyframes", StringComparison.CurrentCultureIgnoreCase)>0)
            {
                List<CssKeyframeSelector> kfslrs = new List<CssKeyframeSelector>();
                while (!over)
                {
                    rule = NextRule(s => CssSelector.Parse<CssKeyframeSelector>(s, (o, l) => new CssKeyframeSelector(o, l)));
                    if (rule != null && rule is CssKeyframeSelector)
                    { kfslrs.Add((CssKeyframeSelector)rule); }
                    RemoveSpace();
                    if (Read(false) == end || over) { Next(); break; }
                }
                return new CssKeyframes(name, kfslrs);
                
            }



            StringBuilder sb = new StringBuilder(start.ToString());
            ch = Read();
            //int f = 0;
            while (!over && ch!=end)
            {
                //if (start == '{' && start==ch)
                //{ f++; }
                sb.Append(ch);
                ch = Read();
                if(ch== '"')
                {
                    do
                    {
                        sb.Append(ch);
                    } while ((ch = Read()) != '"');
                }
                if (ch == '\'')
                {
                    do
                    {
                        sb.Append(ch);
                    } while ((ch = Read()) != '\'');
                }
                if(ch=='{')
                {
                    do
                    {
                        sb.Append(ch);
                    } while ((ch = Read()) != '}');
                    sb.Append(ch);
                    ch = Read();
                }
                //if (f > 0&&ch==end)
                //{
                //    sb.Append(ch);
                //    f--;
                //}
            }
            if(!over)sb.Append(ch);

            if (name.StartsWith("@charset", StringComparison.CurrentCultureIgnoreCase))
            {
                encoding = sb.ToString().Trim(' ', start);
                return null;
            }
            else if (name.StartsWith("@import", StringComparison.CurrentCultureIgnoreCase))
            {
                return new CssImport(sb.ToString());
            }
            return fn == null ? CssSelector.Parse(name + sb.ToString()) : fn(name + sb.ToString());


        
        }

        CssDocument doc;
        public CssDocument Parse()
        {
            if (doc != null) return doc;
            List<CssRule> list = new List<CssRule>();
            while (!over)
            {
                var r = NextRule();
                if (r != null)
                {
                    list.Add(r);
                }
            }
            doc = new CssDocument(list, encoding);
            return doc;
        }
       
    }
}
