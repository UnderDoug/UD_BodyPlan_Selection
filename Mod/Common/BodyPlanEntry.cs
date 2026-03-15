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

        private AnatomyCategory _Category;
        public AnatomyCategory Category => _Category ??= AnatomyCategory.TryGetFor(this, out var category) ? category : null;

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
                        OptionDelegates.Merge(Transformation.OptionDelegates);

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

        public string GetDescription(bool ShowDefault = false, bool ShowSymbols = false)
        {
            SB.Clear();

            string displayName = Anatomy?.Name?.SplitCamelCase();

            if (displayName != null
                && AnatomyConfigurations.GetDisplayName() is string configDisplayName)
                displayName = configDisplayName;

            SB.Append(displayName ?? MISSING_ANATOMY);

            if (SB.ToString() != MISSING_ANATOMY)
            {
                if (ShowDefault
                    && IsDefault)
                    SB.Append(" (default)");

                if (ShowSymbols
                    && !AnatomyConfigurations.IsNullOrEmpty()
                    && AnatomyConfigurations.HasSymbols())
                    SB.Append($" {AnatomyConfigurations.Symbols().Aggregate("", (a, n) => a + n)}");
            }

            return SB.ToString();
        }

        public string GetLongDescription(
            bool Summary = false,
            bool IncludeOpening = false,
            bool IsTrueKin = false
            )
        {
            SB.Clear();

            if (Anatomy == null)
                return SB.ToString();

            string cacheKey = "LongDesc";

            if (Summary)
                cacheKey += $";{nameof(Summary)}";

            if (IncludeOpening)
                cacheKey += $";{nameof(IncludeOpening)}";

            if (IsTrueKin)
                cacheKey += $";{nameof(IsTrueKin)}";

            LongDescriptions ??= new();

            if (!LongDescriptions.ContainsKey(cacheKey)
                || LongDescriptions[cacheKey].IsNullOrEmpty())
            {
                SampleCreature ??= GameObject.CreateSample("Humanoid");
                Anatomy.ApplyTo(SampleCreature.Body);

                if (!Summary)
                    GetLongDescriptionInternal(SB, IncludeOpening, IsTrueKin);
                else
                    GetLongDescriptionSummaryInternal(SB, IncludeOpening, IsTrueKin);

                LongDescriptions[cacheKey] = SB.ToString();
            }
            return LongDescriptions[cacheKey];
        }

        public void GetLongDescriptionOpening(StringBuilder SB, bool Summary = false)
        {
            if (!Summary)
                SB.Append("Includes the following body part slots:");
            else
                SB.Append("Included parts:");
        }

        public void GetLongDescriptionExtras(StringBuilder SB, bool Summary = false)
        {
            if (Summary)
                SB.AppendLine();

            if (Anatomy.HasRecipe())
            {
                if (!Summary)
                    SB.AppendColored("m", "There is a cooking recipe to get this body plan.")
                        .AppendLine()
                        ;
                else
                    SB.AppendColored("m", "Avaialable via cooking");
                SB.AppendLine();
            }

            if (((AnatomyConfigurations?.IsMechanical() ?? false)
                    || Anatomy?.Category == BodyPartCategory.MECHANICAL)
                && Options.EnableRoboticBodyPlansMakingYouRobotic)
            {
                if (!Summary)
                    SB.AppendColored("c", "You will be made mechanical with this body plan.")
                        .AppendLine()
                        ;
                else
                    SB.AppendColored("c", "You are mechanical");
                SB.AppendLine();
            }

            if (!Summary
                && (AnatomyConfigurations?.HasDescriptionAddition() ?? false))
                foreach (string exceptionMessage in AnatomyConfigurations.DescriptionAdditions())
                    SB.Append(exceptionMessage)
                        .AppendLine()
                        .AppendLine()
                        ;

            if (Summary
                && (AnatomyConfigurations?.HasSummaryAddition() ?? false))
                foreach (string exceptionSummary in AnatomyConfigurations.SummaryAdditions())
                    SB.Append(exceptionSummary)
                    .AppendLine()
                    ;
        }

        private StringBuilder GetLongDescriptionInternal(
            StringBuilder SB,
            bool IncludeOpening,
            bool IsTrueKin
            )
        {
            GetLongDescriptionExtras(SB, false);

            if (IncludeOpening)
                GetLongDescriptionOpening(SB, false);

            bool anyHasNatEquip = false;

            SampleCreature.Body.GetLimbTree(
                SB: SB,
                IndentProc: s => "{{K|" + s + "}}",
                BodyPartProc: bp => GetBodyPartString(BodyPart: bp, IsTrueKin: IsTrueKin, ExcludeDefaultBehaviorName: true),
                Treat0DepthPartsAsRoot: true);
            anyHasNatEquip = SampleCreature.Body.GetFirstPart(bp => !bp.VariantTypeModel().DefaultBehavior.IsNullOrEmpty()) != null;

            SB.AppendLine();
            if (IsTrueKin)
                SB
                    .AppendLine()
                    .AppendNoCybernetics(false).Append(" - Incompatible with {{c|cybernetics}}")
                    ;

            if (anyHasNatEquip)
                SB
                    .AppendLine()
                    //.AppendColored("w", "Indicates natural equipment")
                    .AppendColored("w", "Has natural equipment")
                    ;

            return SB.AppendLines(2);
        }

        private StringBuilder GetLongDescriptionSummaryInternal(
            StringBuilder SB,
            bool IncludeOpening,
            bool IsTrueKin
            )
        {
            if (IncludeOpening)
                GetLongDescriptionOpening(SB, true);

            var limbCounts = new Dictionary<BodyPartType, int>();
            if (SampleCreature.Body.GetParts().Select(p => p.VariantTypeModel()) is IEnumerable<BodyPartType> limbs)
                foreach (BodyPartType limb in limbs)
                {
                    if (limbCounts.ContainsKey(limb))
                        limbCounts[limb]++;
                    else
                        limbCounts[limb] = 1;
                }

            foreach ((BodyPartType limb, int count) in limbCounts)
            {
                if (!SB.IsNullOrEmpty())
                    SB.AppendLine();

                string timesColored = "}}x {{Y|";
                string limbName = limb.FinalType ?? limb.Type;
                string limbPluralName = null;

                if (limb.DescriptionPrefix is string prefix)
                    limbName = $"{prefix} {limbName}";

                if (limb.Plural.GetValueOrDefault())
                    limbPluralName = timesColored + limbName;

                if (limbName.EqualsNoCase("feet"))
                    limbPluralName = timesColored + "Worn on Feet";

                if (limbName.EqualsNoCase("foot"))
                    limbPluralName = timesColored + "Feet";

                limbName = timesColored + limbName;

                SB.AppendColored("Y", count.Things(limbName, limbPluralName));

                if (limb.FinalType.EqualsNoCase("body")
                    && SampleCreature.Body.CalculateMobilitySpeedPenalty() is int moveSpeedPenalty
                    && moveSpeedPenalty > 0)
                    SB
                        .Append(' ')
                        .AppendColored("r", $"{-moveSpeedPenalty} MS");

                if (GameObjectFactory.Factory.GetBlueprintIfExists(limb.DefaultBehavior) is GameObjectBlueprint defaultBehvaiour)
                    GetDefaultBehaviorString(SB, defaultBehvaiour, true);

                if (IsTrueKin
                    && (limb?.Category ?? BodyPartCategory.ANIMAL) != BodyPartCategory.ANIMAL)
                    SB.AppendNoCybernetics();
            }
            GetLongDescriptionExtras(SB, true);
            return SB;
        }

        public static StringBuilder GetBodyPartString(
            StringBuilder SB,
            BodyPart BodyPart,
            out bool HasNaturalEquipment,
            bool IsTrueKin = false,
            bool ExcludeDefaultBehaviorName = false
            )
        {
            string defaultBehaviour = BodyPart.VariantTypeModel().DefaultBehavior;
            HasNaturalEquipment = !defaultBehaviour.IsNullOrEmpty();

            string cardinalDescription = BodyPart.GetCardinalDescription();
            string description = BodyPart.VariantTypeModel().Description;

            if (!SB.IsNullOrEmpty())
                SB.AppendLine();

            if (HasNaturalEquipment
                && ExcludeDefaultBehaviorName)
                SB.Append(cardinalDescription.Replace(description, "{{w|" + description + "}}"))
                    //.AppendColored("w", cardinalDescription)
                    ;
            else
                SB.Append(cardinalDescription);

            if (BodyPart.IsVariantType()
                && !cardinalDescription.ContainsNoCase(BodyPart.Type))
                SB
                    .Append(" (")
                    .Append(BodyPart.TypeModel().FinalType)
                    .Append(")")
                    ;

            if (BodyPart.VariantTypeModel().FinalType.EqualsNoCase("body")
                && BodyPart.ParentBody.CalculateMobilitySpeedPenalty() is int moveSpeedPenalty
                && moveSpeedPenalty > 0)
                SB
                    .Append(' ')
                    .AppendColored("r", $"{-moveSpeedPenalty} Move Speed Penalty");

            if (GameObjectFactory.Factory.GetBlueprintIfExists(BodyPart.VariantTypeModel().DefaultBehavior) is GameObjectBlueprint defaultBehvaiour)
            {
                HasNaturalEquipment = true;
                GetDefaultBehaviorString(SB, defaultBehvaiour, ExcludeDefaultBehaviorName);
            }

            if (IsTrueKin
                && !BodyPart.CanReceiveCyberneticImplant())
                SB.AppendNoCybernetics();

            return SB;
        }
        public static string GetBodyPartString(
            BodyPart BodyPart,
            bool IsTrueKin = false,
            bool ExcludeDefaultBehaviorName = false
            )
            => Event.FinalizeString(
                SB: GetBodyPartString(
                    SB: Event.NewStringBuilder(),
                    BodyPart: BodyPart,
                    HasNaturalEquipment: out _,
                    IsTrueKin: IsTrueKin,
                    ExcludeDefaultBehaviorName: ExcludeDefaultBehaviorName));

        public static StringBuilder GetDefaultBehaviorString(
            StringBuilder SB,
            GameObjectBlueprint defaultBehvaiour,
            bool ExcludeName = false
            )
        {
            var sampleDefaultBehaviour = defaultBehvaiour.createSample();

            if (!ExcludeName)
                SB
                    //.Append(" - ")
                    .Append(' ')
                    .AppendColored("w", sampleDefaultBehaviour.ShortDisplayNameStripped);

            var mw = sampleDefaultBehaviour.GetPart<MeleeWeapon>();
            bool mwNotImprovisedAndNull = !(mw?.IsImprovisedWeapon() ?? true);
            string damage = mwNotImprovisedAndNull ? mw?.BaseDamage : null;

            int pVCap = mwNotImprovisedAndNull ? mw?.MaxStrengthBonus ?? 0 : 0;
            int pV = mwNotImprovisedAndNull ? 4 : 0;
            string pVSymbolColor = GetDisplayNamePenetrationColorEvent.GetFor(sampleDefaultBehaviour);

            var armor = sampleDefaultBehaviour.GetPart<Armor>();
            int aV = armor != null ? armor.AV : 0;
            int dV = armor != null ? armor.DV : 0;

            if (armor != null)
                SB.Append(' ').AppendArmor("y", aV, dV);

            if (pV > 0
                && pVCap > 0)
                SB.Append(' ').AppendPV(pVSymbolColor, "y", pV, pVCap);

            if (!damage.IsNullOrEmpty())
                SB.Append(' ').AppendDamage("y", damage);

            sampleDefaultBehaviour?.Obliterate();
            return SB;
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
