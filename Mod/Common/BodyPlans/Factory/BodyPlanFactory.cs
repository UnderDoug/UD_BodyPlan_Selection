using System;
using System.Collections.Generic;
using System.Text;

using XRL;
using XRL.World;

using static UD_BodyPlan_Selection.Mod.Const;
using static UD_BodyPlan_Selection.Mod.BodyPlans.TextElement;
using XRL.UI;
using UD_BodyPlan_Selection.Mod.XML;
using static UD_BodyPlan_Selection.Mod.BodyPlans.BodyPlanCategory;
using System.Linq;

namespace UD_BodyPlan_Selection.Mod.BodyPlans
{
    [HasModSensitiveStaticCache]
    public class BodyPlanFactory
        : IXmlFactory<Symbol>
        , IXmlFactory<BodyPlanCategory>
        , IXmlFactory<TextShader>
        , IXmlFactory<BodyPlanEntry>
        , IXmlFactory<BodyPlanRenderable>
        , IXmlFactory<TransformationData>
    {
        public Dictionary<string, Symbol> SymbolsByName;
        Dictionary<string, Symbol> IXmlFactory<Symbol>.EntriesByName
        {
            get => SymbolsByName;
            set => SymbolsByName = value;
        }
        public List<Symbol> Symbols => SymbolsByName?.Values?.ToList();

        public Dictionary<string, BodyPlanCategory> CategoriesByName;
        Dictionary<string, BodyPlanCategory> IXmlFactory<BodyPlanCategory>.EntriesByName
        {
            get => CategoriesByName;
            set => CategoriesByName = value;
        }
        public List<BodyPlanCategory> Categories => CategoriesByName?.Values?.ToList();

        Dictionary<string, TextShader> IXmlFactory<TextShader>.EntriesByName
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        Dictionary<string, BodyPlanEntry> IXmlFactory<BodyPlanEntry>.EntriesByName
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        Dictionary<string, BodyPlanRenderable> IXmlFactory<BodyPlanRenderable>.EntriesByName
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        Dictionary<string, TransformationData> IXmlFactory<TransformationData>.EntriesByName
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        private static BodyPlanFactory _Factory;
        public static BodyPlanFactory Factory
        {
            get
            {
                if (_Factory == null)
                {
                    _Factory = new();
                    Loading.LoadTask("Loading TextElements.xml", _Factory.LoadTextElements);
                    Loading.LoadTask("Loading BodyPlans.xml", _Factory.LoadBodyPlans);
                }
                return _Factory;
            }
        }

        public void LoadTextElements()
        {
            Utils.ThisMod.Error(new NotImplementedException($"Custom parsing of TextElement is yet to be implemented."));
        }

        public void LoadBodyPlans()
        {
            Utils.ThisMod.Error(new NotImplementedException($"Custom parsing of BodyPlanEntry is yet to be implemented."));
        }

        protected string LoadFromDataSignature<T>()
            where T : IXmlLoaded<T>, new()
            => $"{nameof(BodyPlanFactory)}.{nameof(LoadFromData)}({nameof(XmlDataLoader.XmlData<T>)})";

        protected string LoadFromDataMissingNode<T>(string NodeName)
            where T : IXmlLoaded<T>, new()
            => $"{LoadFromDataSignature<Symbol>()} attempted to load {typeof(T).Name} without '{NodeName}'";

        public Symbol LoadFromData(XmlDataLoader.XmlData<Symbol> XmlData)
        {
            var attributes = XmlData.Attributes ?? new Dictionary<string, string>();

            if (!char.TryParse(attributes.GetValue(nameof(Symbol.Color)), out char color))
                char.TryParse(XmlData.GetNamedChildNode(nameof(color))?.TextLines?[0]?.Trim(), out color);

            if (!char.TryParse(attributes.GetValue(nameof(Symbol.Value)), out char value)
                && (XmlData.GetNamedChildNode(nameof(color))?.TextLines?[0]?.Trim() is not string valueString
                    || !char.TryParse(Sidebar.ToCP437(valueString), out value)))
            {
                XmlData.Mod.Error($"{LoadFromDataMissingNode<Symbol>(nameof(Symbol.Value))}");
                return default;
            }

            return new(XmlData.Name, color, value);
        }

        public BodyPlanCategory LoadFromData(XmlDataLoader.XmlData<BodyPlanCategory> XmlData)
        {
            throw new NotImplementedException();
        }

        public TextShader LoadFromData(XmlDataLoader.XmlData<TextShader> XmlData)
        {
            throw new NotImplementedException();
        }

        public BodyPlanEntry LoadFromData(XmlDataLoader.XmlData<BodyPlanEntry> XmlData)
        {
            throw new NotImplementedException();
        }
        public BodyPlanRenderable LoadFromData(XmlDataLoader.XmlData<BodyPlanRenderable> XmlData)
        {
            throw new NotImplementedException();
        }

        public TransformationData LoadFromData(XmlDataLoader.XmlData<TransformationData> XmlData)
        {
            throw new NotImplementedException();
        }

        private XmlDataLoader<T> GetXmlDataLoaderInternal<T>()
            where T : IXmlLoaded<T>, new()
            => new();

        XmlDataLoader<Symbol> IXmlFactory<Symbol>.GetXmlDataLoader()
            => GetXmlDataLoaderInternal<Symbol>();

        XmlDataLoader<BodyPlanCategory> IXmlFactory<BodyPlanCategory>.GetXmlDataLoader()
            => GetXmlDataLoaderInternal<BodyPlanCategory>();

        XmlDataLoader<TextShader> IXmlFactory<TextShader>.GetXmlDataLoader()
            => GetXmlDataLoaderInternal<TextShader>();

        XmlDataLoader<BodyPlanEntry> IXmlFactory<BodyPlanEntry>.GetXmlDataLoader()
            => GetXmlDataLoaderInternal<BodyPlanEntry>();

        XmlDataLoader<BodyPlanRenderable> IXmlFactory<BodyPlanRenderable>.GetXmlDataLoader()
            => GetXmlDataLoaderInternal<BodyPlanRenderable>();

        XmlDataLoader<TransformationData> IXmlFactory<TransformationData>.GetXmlDataLoader()
            => GetXmlDataLoaderInternal<TransformationData>();

        Symbol IXmlFactory<Symbol>.LoadFromData(XmlDataLoader.XmlData<Symbol> XmlData)
        {
            throw new NotImplementedException();
        }

        BodyPlanCategory IXmlFactory<BodyPlanCategory>.LoadFromData(XmlDataLoader.XmlData<BodyPlanCategory> XmlData)
        {
            throw new NotImplementedException();
        }
    }
}
