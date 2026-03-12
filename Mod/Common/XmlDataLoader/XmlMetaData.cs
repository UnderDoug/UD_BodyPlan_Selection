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
    public abstract class XmlMetaData
    {
        public string RootNode;

        public string DataNodeName;

        public string NameAttribute;

        public bool KnownAttributesOnly;
        public bool KnownNodesOnly;

        public bool KnownOnly;

        public List<string> KnownAttributes;
        public List<string> KnownNodes;
        
        public bool IsNamed => !NameAttribute.IsNullOrEmpty();

        public bool IsUnique;

        public bool IsInheritable;

        public bool IsMergable;

        public XmlMetaData()
            : base()
        { }

        public virtual IEnumerable<string> GetKnownAttributes()
        {
            using var alreadyYielded = ScopeDisposedList<string>.GetFromPool();

            if (!KnownAttributes.IsNullOrEmpty())
            {
                foreach (var attribute in KnownAttributes)
                {
                    if (!alreadyYielded.Contains(attribute))
                    {
                        alreadyYielded.Add(attribute);
                        yield return attribute;
                    }
                }
            }
        }

        public virtual IEnumerable<string> GetKnownNodes()
        {
            using var alreadyYielded = ScopeDisposedList<string>.GetFromPool();

            if (!KnownNodes.IsNullOrEmpty())
            {
                foreach (var node in KnownNodes)
                {
                    if (!alreadyYielded.Contains(node))
                    {
                        alreadyYielded.Add(node);
                        yield return node;
                    }
                }
            }
        }

        public virtual bool IsKnownAttribute(string AttributeName)
        {
            if (GetKnownAttributes() is not IEnumerable<string> knownNodes)
                return !KnownOnly
                    && !KnownAttributesOnly;

            return knownNodes.Any(s => s == AttributeName);
        }

        public virtual bool IsKnownNode(string NodeName)
        {
            if (GetKnownNodes() is not IEnumerable<string> knownNodes)
                return !KnownOnly
                    && !KnownNodesOnly;

            return knownNodes.Any(s => s == NodeName);
        }
    }
}
