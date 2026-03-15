using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ConsoleLib.Console;

using XRL;
using XRL.World;
using XRL.World.Anatomy;
using XRL.World.Parts;

using Event = XRL.World.Event;

using UD_ChooseYourBodyPlan.Mod;
using static UD_ChooseYourBodyPlan.Mod.AnatomyConfiguration;

namespace UD_ChooseYourBodyPlan.Mod
{
    public class BodyPlanEntry: ILoadFromDataBucket<BodyPlanEntry>
    {
        public static string MISSING_ANATOMY => nameof(MISSING_ANATOMY);

        public static BodyPlanFactory Factory => BodyPlanFactory.Factory;

        public string BaseDataBucketBlueprint => Const.BODYPLAN_ENTRY_BLUEPRINT;

        public string CacheKey => Anatomy?.Name;

        public Anatomy Anatomy;

        protected string CategoryOverride;

        private AnatomyCategoryEntry _Category;
        public AnatomyCategoryEntry Category => _Category ??= AnatomyCategoryEntry.TryGetFor(this, out var category) ? category : null;

        public string DisplayName;

        public string DisplayNameStripped => GetDescription()?.Strip();

        public BodyPlanRender Render;

        public OptionDelegates OptionDelegates;

        private TransformationData _Transformation;
        public TransformationData Transformation
        {
            get
            {
                if (WantsTransformation
                    && (Factory?.TransformationDataInitialized ?? false)
                    && _Transformation == null)
                {
                    WantsTransformation = false;
                    _Transformation = Factory.GetTransformationData(this);
                    if (_Transformation != null)
                        Utils.MergeDistinctInCollection(ref OptionDelegates, Transformation.OptionDelegates);

                }
                return _Transformation;
            }
        }
        protected bool WantsTransformation;

        public bool IsDefault;

        private Dictionary<string, string> LongDescriptions;

        public string LongDescription => GetLongDescription(IncludeOpening: true);
        public string LongDescriptionNoOpen => GetLongDescription();
        public string LongDescriptionSummary => GetLongDescription(Summary: true, IncludeOpening: true);
        public string LongDescriptionNoOpenSummary => GetLongDescription(Summary: true);
        public string LongDescriptionTK => GetLongDescription(IncludeOpening: true, IsTrueKin: true);
        public string LongDescriptionNoOpenTK => GetLongDescription(IsTrueKin: true);
        public string LongDescriptionTKSummary => GetLongDescription(Summary: true, IncludeOpening: true, IsTrueKin: true);
        public string LongDescriptionNoOpenTKSummary => GetLongDescription(Summary: true, IsTrueKin: true);

        private HashSet<string> TextElementsNames;

        private List<TextElements> _TextElements;
        public List<TextElements> TextElements
        {
            get
            {
                if (WantsTextElements
                    && Factory.TextElementsInitialized
                    && _TextElements.IsNullOrEmpty())
                {
                    WantsTextElements = false;
                    _TextElements = new();
                    if (!TextElementsNames.IsNullOrEmpty())
                    {
                        foreach (var textElementsName in TextElementsNames)
                        {
                            if (Factory.TextElementsByName.TryGetValue(textElementsName, out var textElements))
                            {
                                _TextElements.Add(textElements);
                            }
                        }
                    }
                }
                return _TextElements;
            }
        }
        protected bool WantsTextElements;

        public Dictionary<string, string> Tags;

        public int RandomWeight;

        protected static StringBuilder SB = new();

        protected static GameObject SampleCreature = null;

        #region Obsolete

        private List<AnatomyConfiguration> _AnatomyConfigurations;
        public List<AnatomyConfiguration> AnatomyConfigurations => _AnatomyConfigurations ??= new(Utils.GetAnatomyConfigurations(this));

        #endregion

        public BodyPlanEntry()
        {
            Anatomy = null;
            _Category = null;
            _AnatomyConfigurations = null;
            Render = null;

            _Transformation = null;
            WantsTransformation = true;

            IsDefault = false;

            TextElementsNames = null;
            _TextElements = null;
            WantsTextElements = true;

            Tags = null;

            LongDescriptions = null;
        }
        public BodyPlanEntry(Anatomy Anatomy, bool IsDefault, BodyPlanRender Render)
            : this()
        {
            this.Anatomy = Anatomy;
            this.Render = Render;
            this.IsDefault = IsDefault;

            _ = Category;
        }
        public BodyPlanEntry(Anatomy Anatomy, BodyPlanRender Renderable)
            : this(Anatomy, false, Renderable)
        {
        }
        public BodyPlanEntry(Anatomy Anatomy, bool IsDefault)
            : this(Anatomy, IsDefault, null)
        {
        }
        public BodyPlanEntry(Anatomy Anatomy)
            : this(Anatomy, false, null)
        {
        }

        public BodyPlanEntry LoadFromDataBucket(GameObjectBlueprint DataBucket)
        {
            if (!ILoadFromDataBucket<BodyPlanEntry>.CheckIsValidDataBucket(this, DataBucket))
                return null;

            if (DataBucket.TryGetTagValueForData(nameof(Anatomy), out string anatomyName))
            {
                if (Anatomies.GetAnatomy(anatomyName) is not Anatomy anatomy)
                    return null;

                Anatomy = anatomy;
            }

            DataBucket.AssignStringFieldFromTag(nameof(Category), ref CategoryOverride);
            DataBucket.AssignStringFieldFromTag(nameof(CategoryOverride), ref CategoryOverride);
            DataBucket.AssignStringFieldFromTag(nameof(DisplayName), ref DisplayName);

            Render = new BodyPlanRender().LoadFromDataBucket(DataBucket);

            OptionDelegates.ParseDataBucket(DataBucket);

            if (DataBucket.GetTextElementsTags() is IEnumerable<KeyValuePair<string, string>> textElementsTags)
            {
                TextElementsNames = new();
                foreach ((var textElementsName, var _) in textElementsTags)
                    TextElementsNames.Add(textElementsName);
            }

            if (DataBucket.TryGetTagValueForData(nameof(RandomWeight), out string randomWeight)
                && !int.TryParse(randomWeight, out RandomWeight))
                RandomWeight = 5;

            Tags = new();
            foreach ((string tagName, string tagValue) in DataBucket.Tags)
                Tags[tagName] = tagValue;

            return this;
        }

        public BodyPlanEntry Merge(BodyPlanEntry Other)
        {
            throw new NotImplementedException();
        }

        public BodyPlanEntry Clone()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
            => $"{GetDescription(ShowDefault: true, ShowSymbols: true)}{(Render?.Tile is string tile ? " " + tile : null)}";

        public void ClearLongDescriptionCaches()
        {
            LongDescriptions.Clear();
        }

        public BodyPlanRender GetRenderable()
        {
            if (Render == null
                && Anatomy != null)
            {
                if (AnatomyConfigurations?.FirstTransformationOrDefault() is TransformationData xForm
                    && !xForm.Tile.IsNullOrEmpty()
                    && xForm.DetailColor != '\0')
                    Render = new(xForm, true);
                else
                if (BodyPlanRender.BodyPlanRenderables?.ContainsKey(Anatomy.Name) ?? false)
                    Render = BodyPlanRender.BodyPlanRenderables[Anatomy.Name];
                else
                    Render = new(GetExampleBlueprint()?.GetRenderable(), false);
            }

            return Render;
        }
        public void OverrideRender(BodyPlanRender Render)
        {
            if (Render != null)
                this.Render = Render;
        }

        public bool HasTag(string Name)
        {
            throw new NotImplementedException();
        }

        public string GetTag(string Name)
        {
            throw new NotImplementedException();
        }

        public bool HasMatchingAnatomy(GameObjectBlueprint Blueprint)
            => Anatomy != null
            && Blueprint.GetAnatomyName() is string anatomy
            && anatomy == Anatomy.Name
            ;
        public bool ObjectAnimatesWithAnatomy(GameObjectBlueprint Blueprint)
            => Anatomy != null
            && Blueprint.TryGetTag("BodyType", out string bodyType)
            && Anatomy.Name == bodyType
            ;
        public bool InheritsFromAnatomy(GameObjectBlueprint Blueprint)
            => Anatomy != null
            && Blueprint.InheritsFrom(Anatomy.Name)
            ;

        public IEnumerable<GameObjectBlueprint> GetExampleBlueprints()
        {
            var blueprints = Utils.GenerallyEligbleForDisplayBlueprints;

            if (blueprints
                ?.Where(HasMatchingAnatomy) is IEnumerable<GameObjectBlueprint> objectsWithAnatomy
                && !objectsWithAnatomy.IsNullOrEmpty())
                return objectsWithAnatomy;

            if (blueprints
                ?.Where(ObjectAnimatesWithAnatomy) is IEnumerable<GameObjectBlueprint> objectsAnimatingWithAnatomy
                && !objectsAnimatingWithAnatomy.IsNullOrEmpty())
                return objectsAnimatingWithAnatomy;

            if (blueprints
                ?.Where(InheritsFromAnatomy) is IEnumerable<GameObjectBlueprint> objectsInheritingAnatomy
                && !objectsInheritingAnatomy.IsNullOrEmpty())
                return objectsInheritingAnatomy;

            return new GameObjectBlueprint[0];
        }

        public GameObjectBlueprint GetExampleBlueprint()
            => GetExampleBlueprints()?.GetRandomElementCosmetic()
            ?? GameObjectFactory.Factory.GetBlueprintIfExists("Mimic")
            ;

        public void Dispose()
        {
            Render.Dispose();
            Render = null;

            OptionDelegates.Clear();
            OptionDelegates = null;

            LongDescriptions.Clear();
            LongDescriptions = null;

            Tags.Clear();
            Tags = null;
        }
    }
}
