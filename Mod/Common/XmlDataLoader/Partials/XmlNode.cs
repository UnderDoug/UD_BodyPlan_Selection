using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using XRL;
using XRL.Collections;

using static UD_BodyPlan_Selection.Mod.BodyPlans.TextElement;

namespace UD_BodyPlan_Selection.Mod.XML
{

    public abstract partial class XmlDataLoader
    {
        public abstract class XmlNode
        {
            public string NodeName;

            public Dictionary<string, string> Attributes;
            public List<XmlNode> Children;
            public List<string> TextLines;

            public XmlNode()
            {
                NodeName = null;
                Attributes = new();
                Children = new();
            }
            protected XmlNode(XmlDataHelper Reader)
                : this()
            {
                NodeName = Reader.Name;
                ReadNodeInternal(Reader);
            }
            protected XmlNode(XmlNode Source)
                : this()
            {
                NodeName = Source.NodeName;
                Children = new(Source.Children);
                Attributes = new(Source.Attributes);
            }

            public static XmlNode ReadNode(XmlNode Node, XmlDataHelper Reader)
            {
                Node.ReadNodeInternal(Reader);
                return Node;
            }

            protected void ReadNodeInternal(XmlDataHelper Reader)
            {
                if (Reader.HasAttributes)
                {
                    AssignFromAttributesByName(Reader);
                    HandleNodeAttributes(Reader);
                }

                if (Reader.NodeType == XmlNodeType.EndElement
                    || Reader.IsEmptyElement)
                    return;

                while (Reader.Read())
                {
                    if (Reader.NodeType == XmlNodeType.EndElement)
                    {
                        if (Reader.Name != ""
                            && Reader.Name != NodeName)
                            throw new Exception($"Unexpected end node for {Reader.Name}");

                        HandleNodeTypeEndElement(Reader);
                        return;
                    }

                    if (Reader.NodeType == XmlNodeType.Element
                        && HandleNodeTypeElement(Reader))
                        continue;

                    if (Reader.NodeType == XmlNodeType.Comment
                        && HandleNodeTypeComment(Reader))
                        continue;

                    if (Reader.NodeType == XmlNodeType.Text
                        && HandleNodeTypeText(Reader))
                        continue;

                    Reader.modInfo.Error($"{Reader.SanitizedBaseURI()}: Unknown problem reading object: {Reader.NodeType}");
                }
            }

            public virtual void AssignFromAttributesByName(XmlDataHelper Reader)
            {
            }

            public void HandleNodeAttributes(XmlDataHelper Reader)
            {
                Reader.MoveToFirstAttribute();
                do
                {
                    if (!HandleNodeAttribute(Reader))
                        break;
                }
                while (Reader.MoveToNextAttribute());
                Reader.MoveToElement();
            }

            public virtual bool HandleNodeAttribute(XmlDataHelper Reader)
            {
                return true;
            }

            public virtual void HandleNodeTypeEndElement(XmlDataHelper Reader)
            {
            }

            public virtual bool HandleNodeTypeElement(XmlDataHelper Reader)
            {
                return true;
            }

            public virtual bool HandleNodeTypeText(XmlDataHelper Reader)
            {
                if (Reader.ReadString().Split('\n', StringSplitOptions.RemoveEmptyEntries) is string[] textLines)
                {
                    TextLines ??= new();
                    for (int i = 0; i < textLines.Length; i++)
                    {
                        if (textLines[i].Replace("\r", "") is string textLine)
                            TextLines.Add(textLine);
                    }
                }
                return true;
            }

            public virtual bool HandleNodeTypeComment(XmlDataHelper Reader)
            {
                return true;
            }

            public abstract bool SameAs(XmlNode Other);

            protected virtual void AddChild<T>(T ChildNode)
                where T : XmlNode
            {
                if (Children.FirstOrDefault(ChildNode.SameAs) is T existingNode)
                    Children.Remove(existingNode);

                Children.Add(ChildNode);
            }

            public virtual void Merge(XmlNode Other)
            {
                NodeName = Other.NodeName;

                Utils.Merge(This: Other.Attributes, Into: ref Attributes);
            }

            public virtual XmlNode Clone()
            {
                var clone = Activator.CreateInstance(GetType()) as XmlNode;

                clone.NodeName = NodeName;
                clone.Attributes = new();
                clone.Children = new();

                foreach ((string name, string value) in Attributes)
                    clone.Attributes.Add(name, value);

                foreach (var childNode in Children)
                    clone.Children.Add(childNode.Clone());

                return clone;
            }

            public string GetAttribute(string Name)
                => Attributes.GetValue(Name);

            public bool HasAttribute(string Name)
                => Attributes.ContainsKey(Name);

            public IEnumerable<XmlNode> GetChildNodes(Predicate<XmlNode> Filter)
            {
                foreach (var child in Children)
                    if (Filter == null
                        || Filter(child))
                        yield return child;
            }

            public IEnumerable<XmlNode> GetChildNodes(string NodeName)
                => GetChildNodes(n => n.NodeName == NodeName);

        }
    }
}
