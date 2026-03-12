using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using XRL;

using static UD_BodyPlan_Selection.Mod.Const;

namespace UD_BodyPlan_Selection.Mod.XML
{
    public abstract partial class XmlDataLoader
    {
        public static Action<object> HandleError;
        public static Action<object> HandleWarning;

        public XmlDataLoader()
        {
            HandleError = Utils.ThisMod.Error;
            HandleWarning = Utils.ThisMod.Warn;
        }

        public abstract XmlMetaData GetMetaData();

        protected void SetLoggers(ModInfo ModInfo)
        {
            if (ModInfo != Utils.ThisMod)
            {
                HandleError = ModInfo.Error;
                HandleWarning = ModInfo.Warn;
            }
            else
            {
                HandleError = Utils.ThisMod.Error;
                HandleWarning = Utils.ThisMod.Warn;
            }
        }

        public void LoadXMLRootNodes()
        {
            string root = GetMetaData().RootNode;
            foreach (var reader in DataManager.YieldXMLStreamsWithRoot(root))
            {
                SetLoggers(reader.modInfo);
                try
                {
                    ReadRootXML(reader, root);
                }
                catch (Exception message)
                {
                    MetricsManager.LogPotentialModError(reader.modInfo, message);
                }
            }
        }

        public void ReadRootXML(
            XmlDataHelper Reader,
            string RootNode
            )
        {
            bool any = false;
            try
            {
                Reader.WhitespaceHandling = WhitespaceHandling.None;
                while (Reader.Read())
                {
                    if (Reader.Name == RootNode)
                    {
                        any = true;
                        ReadRootNode(Reader, RootNode);
                    }
                }
            }
            catch (Exception innerException)
            {
                throw new Exception($"{Reader.FileLinePos()}", innerException);
            }
            finally
            {
                Reader.Close();
            }
            if (!any)
                HandleError($"No <{RootNode}> tag found in {Reader.SanitizedBaseURI()}");
        }

        public abstract int ReadRootNode(XmlDataHelper Reader, string RootNode);

        public virtual XmlData<T> ReadXmlDataNode<T>(XmlDataHelper Reader)
            where T : IXmlLoaded<T>, new()
            => XmlNode<T>.ReadNode<XmlData<T>>(Reader);

        public virtual XmlNode<T> ReadXmlNode<T>(XmlDataHelper Reader)
            where T : IXmlLoaded<T>, new()
            => XmlNode<T>.ReadNode<XmlNode<T>>(Reader);

        public abstract int ReadDataNode(XmlDataHelper Reader);
    }
}
