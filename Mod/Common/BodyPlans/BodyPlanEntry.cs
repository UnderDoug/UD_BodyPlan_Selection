using System;
using System.Collections.Generic;
using System.Text;

using UD_BodyPlan_Selection.Mod.XML;

using XRL;

namespace UD_BodyPlan_Selection.Mod.BodyPlans
{
    public class BodyPlanEntry : IXmlLoaded<BodyPlanEntry>
    {
        public IXmlFactory<BodyPlanEntry> Factory => BodyPlanFactory.Factory;

        public XmlMetaData<BodyPlanEntry> XmlMetaData => new(false, false)
        {
            DataNodeName = "bodyplan",
            NameAttribute = nameof(Name),
            KnownAttributes = new()
            {
                "Name",
                "DisplayName",
                "Category",
                "OptionID",
                "Load",
            },
            KnownNodes = new()
            {
                "tag",
                "base",
                "textElement",
                "dynamic",
                "optionID",
            },
            XmlLoadedNodes = new()
            {
                { "render", delegate (XmlDataHelper Reader)
                    {
                        return IXmlLoaded<BodyPlanRenderable>.ReadXmlNode(Reader);
                    } },
                { "transformation", IXmlLoaded<TransformationData>.ReadXmlNode },
            },
            IsInheritable = true,
            IsMergable = true,
        };

        public bool XmlMetaDataFromFieldReflection => false;

        public string Name;
        public string DisplayName;
        public BodyPlanRenderable Renderable;
        public TransformationData Transformation;
        public List<TextElement> TextElements;

        public bool IsBase;

        public BodyPlanEntry()
        {
        }
    }
}
