using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using XRL;

using static UD_BodyPlan_Selection.Mod.Const;

namespace UD_BodyPlan_Selection.Mod.XML
{
    public class XmlDataLoader<T> : XmlDataLoader
        where T : IXmlLoaded<T>, new()
    {
        private Dictionary<string, XmlData<T>> RawNamedDataNodes;

        protected T Instance;

        protected XmlMetaData<T> MetaData => GetMetaData() as XmlMetaData<T>;

        private int NameCounter;

        private string NextAnonymousName => $"{typeof(T).Name}{NameCounter++}";

        public XmlDataLoader()
            : base()
        {
            RawNamedDataNodes = new();
        }

        public override XmlMetaData GetMetaData()
            => (Instance ??= new T()).XmlMetaData;

        public override int ReadRootNode(XmlDataHelper Reader, string RootNode)
        {
            int num = 0;
            while (Reader.Read())
            {
                string nodeName = Reader.Name;
                if (Reader.NodeType == XmlNodeType.Element)
                {
                    if (!MetaData.IsKnownNode(nodeName))
                        HandleWarning($"{Reader.FileLinePos()}, Unknown node '{nodeName}', may be skipped during bake");

                    num += ReadDataNode(Reader);
                    continue;
                }

                if (Reader.NodeType != XmlNodeType.Comment)
                {
                    if (nodeName == RootNode
                        && Reader.NodeType == XmlNodeType.EndElement)
                        return num;

                    throw new Exception($"{Reader.FileLinePos()}, Unknown node '{nodeName}'");
                }
            }
            NameCounter = 0;
            return num;
        }

        public override int ReadDataNode(XmlDataHelper Reader)
        {
            if (ReadXmlDataNode<T>(Reader) is not XmlData<T> xmlData)
                return 0;

            string nodeName = Reader.Name;

            if (MetaData.IsNamed)
            {
                string name = xmlData.Name;
                if (xmlData.Load > XmlNode<T>.LoadType.Replace)
                {
                    if (RawNamedDataNodes.TryGetValue(name, out var existingNode))
                        existingNode.Merge(xmlData);
                    else
                    if (xmlData.Load == XmlNode<T>.LoadType.Merge)
                        HandleError($"{Reader.FileLinePos()}, Attempt to merge with {name} which is an unknown {nodeName}, node discarded");
                }
                else
                    RawNamedDataNodes[xmlData.Name] = xmlData;
            }
            else
                RawNamedDataNodes.Add(NextAnonymousName,xmlData);

            return 1;
        }

        public Dictionary<string, XmlData<T>> GetRawNodes()
            => new(RawNamedDataNodes);
    }
}
