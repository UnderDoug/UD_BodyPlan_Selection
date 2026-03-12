using System;
using System.Collections.Generic;
using System.Text;

using XRL;

namespace UD_BodyPlan_Selection.Mod.XML
{
    public interface IXmlLoaded<T>
        where T : IXmlLoaded<T>, new()
    {
        IXmlFactory<T> Factory { get; }

        XmlMetaData<T> XmlMetaData { get; }

        XmlDataLoader.XmlData<T> ReadXmlDataNode(XmlDataHelper Reader)
            => Factory.GetXmlDataLoader().ReadXmlDataNode<T>(Reader);

        XmlDataLoader.XmlNode ReadXmlNode(XmlDataHelper Reader)
            => Factory.GetXmlDataLoader().ReadXmlNode<T>(Reader);
    }
}
