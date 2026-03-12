using System;
using System.Collections.Generic;
using System.Text;

namespace UD_BodyPlan_Selection.Mod.XML
{
    public interface IXmlFactory<T>
        where T : IXmlLoaded<T>, new()
    {
        Dictionary<string, T> EntriesByName { get; set; }

        XmlDataLoader<T> GetXmlDataLoader();

        void ParseDataNodesFromLoader(Dictionary<string, XmlDataLoader.XmlData<T>> RawNamedDataNodes)
        {
            foreach ((var name, var data) in RawNamedDataNodes)
                if (LoadFromData(data) is T loadedData)
                {
                    EntriesByName ??= new();
                    EntriesByName.Add(name, loadedData);
                }
        }

        T LoadFromData(XmlDataLoader.XmlData<T> XmlData);
    }
}
