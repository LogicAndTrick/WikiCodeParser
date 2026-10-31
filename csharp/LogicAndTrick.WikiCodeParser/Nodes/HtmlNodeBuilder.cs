using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogicAndTrick.WikiCodeParser.Nodes
{
    public class HtmlNodeBuilder
    {
        private readonly Dictionary<string, string> _attributes;
        private readonly List<string> _props;
        private readonly HashSet<string> _classes;
        private readonly Dictionary<string, string> _styles;

        public string Tag { get; }
        public IReadOnlyDictionary<string, string> Attributes => _attributes;
        public IReadOnlyList<string> Props => _props;
        public IReadOnlyCollection<string> Classes => _classes;
        public IReadOnlyDictionary<string, string> Styles => _styles;
        public List<INode> Contents { get; set; }
        
        public static HtmlNodeBuilder Make(string tag) => new HtmlNodeBuilder(tag);

        public HtmlNodeBuilder(string tag)
        {
            if (!HtmlHelper.ValidateTagName(tag)) throw new ArgumentException("Invalid tag.");

            Tag = tag;
            _attributes = new Dictionary<string, string>();
            _props = new List<string>();
            _classes = new HashSet<string>();
            _styles = new Dictionary<string, string>();
            Contents = new List<INode>();
        }

        public HtmlNodeBuilder If(bool test, Action<HtmlNodeBuilder> action)
        {
            if (test) action(this);
            return this;
        }

        public HtmlNodeBuilder AddClass(params string[] classes) => AddClasses(classes);

        public HtmlNodeBuilder AddClasses(IEnumerable<string> classes)
        {
            _classes.UnionWith(classes);
            return this;
        }

        /// <summary>
        /// Styles are not sanitised, be sure to handle them with care!
        /// </summary>
        public HtmlNodeBuilder AddStyle(string prop, string val)
        {
            if (!HtmlHelper.ValidateStylePropertyName(prop)) throw new ArgumentException("Invalid property.");
            if (!HtmlHelper.ValidateStyleValue(val)) throw new ArgumentException("Invalid value.");
            _styles[prop] = val;
            return this;
        }

        public HtmlNodeBuilder Attr(string key, string val)
        {
            if (!HtmlHelper.ValidateAttributeName(key)) throw new ArgumentException("Invalid attribute.");
            if ((key == "href" || key == "src") && !HtmlHelper.ValidateUrl(val)) throw new ArgumentException("Invalid url.");
            if (key == "class") throw new ArgumentException("Use AddClass instead.");
            if (key == "style") throw new ArgumentException("Use AddStyle instead.");
            _attributes[key] = val ?? "";
            return this;
        }

        public HtmlNodeBuilder DelAttr(string key)
        {
            if (key != null) _attributes.Remove(key);
            return this;
        }

        public HtmlNodeBuilder Prop(string prop, bool on = true)
        {
            if (!HtmlHelper.ValidateAttributeName(prop)) throw new ArgumentException("Invalid prop name.");
            if (prop == "class") throw new ArgumentException("Use AddClass instead.");
            if (prop == "style") throw new ArgumentException("Use AddStyle instead.");
            _props.Remove(prop);
            if (on) _props.Add(prop);
            return this;
        }

        public HtmlNodeBuilder Add(INode node)
        {
            Contents.Add(node);
            return this;
        }

        public HtmlNodeBuilder Add(IEnumerable<INode> nodes)
        {
            Contents.AddRange(nodes ?? new INode[0]);
            return this;
        }

        public HtmlNode ToNode(
            bool isBlockNode = false,
            string plainBefore = "",
            string plainAfter = "",
            bool selfClosing = false
        )
        {
            if (Contents.Count > 0) selfClosing = false;

            var tag = HtmlHelper.ValidateTagName(Tag) ? Tag : "div";
            INode content = Contents.Count == 1 ? Contents[0] : new NodeCollection(Contents);
            var sb = new StringBuilder("<");
            sb.Append(tag);
            if (_classes.Count > 0)
            {
                sb.Append(" class=\"");
                sb.Append(HtmlHelper.AttributeEncode(string.Join(" ", _classes.OrderBy(x => x))));
                sb.Append("\"");
            }
            if (_styles.Count > 0)
            {
                sb.Append(" style=\"");
                foreach (var prop in _styles.Keys.OrderBy(x => x))
                {
                    if (!HtmlHelper.ValidateStylePropertyName(prop)) continue;
                    if (!HtmlHelper.ValidateStyleValue(_styles[prop])) continue;
                    var kv = prop + ":" + _styles[prop] + ";";
                    sb.Append(HtmlHelper.AttributeEncode(kv));
                }
                sb.Append("\"");
            }
            foreach (var attr in _attributes.Keys.OrderBy(x => x))
            {
                if (attr == "class" || attr == "style") continue;
                if (!HtmlHelper.ValidateAttributeName(attr)) continue;
                var val = _attributes[attr];
                if (attr == "href" || attr == "src")
                {
                    if (!HtmlHelper.ValidateUrl(_attributes[attr])) continue;
                    else val = HtmlHelper.SanitiseUrl(val);
                }
                val = HtmlHelper.AttributeEncode(val);
                sb.Append(' ').Append(attr).Append("=\"").Append(val).Append("\"");
            }
            foreach (var prop in _props.OrderBy(x => x))
            {
                if (prop == "class" || prop == "style" || _attributes.ContainsKey(prop)) continue;
                if (!HtmlHelper.ValidateAttributeName(prop)) continue;
                sb.Append(' ').Append(prop);
            }
            var after = "</" + tag + ">";
            if (selfClosing)
            {
                sb.Append(" /");
                after = "";
            }
            sb.Append(">");

            return new HtmlNode(sb.ToString(), content, after)
            {
                IsBlockNode = isBlockNode,
                PlainBefore = plainBefore,
                PlainAfter = plainAfter
            };
        }
    }
}