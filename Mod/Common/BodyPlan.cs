using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRL;
using XRL.World;
using XRL.World.Anatomy;
using XRL.World.Parts;

namespace UD_ChooseYourBodyPlan.Mod
{
    public class BodyPlan
    {
        public class LimbTreeBranch
        {
            public static bool IsTrueKin = Utils.IsTruekinEmbarking;

            public string Indent;
            public string CardinalDescription;
            public string Description;
            public string FinalType;
            public string NaturalEquipment;
            public string Extra;
            public bool HasNaturalEquipment => !NaturalEquipment.IsNullOrEmpty();
            public bool NoCyber;

            public override string ToString()
            {
                string output = CardinalDescription;
                if (HasNaturalEquipment)
                    output = output.Replace(Description, "{{w|" + Description + "}}");

                if (!FinalType.IsNullOrEmpty())
                    output = $"{output} ({FinalType})";

                if (HasNaturalEquipment)
                    output = $"{output} {NaturalEquipment}";

                if (!Extra.IsNullOrEmpty())
                    output = $"{output} {Extra}";

                output = $"{Indent}{output}";

                if (IsTrueKin
                    && NoCyber
                    && BodyPlanFactory.Factory
                        ?.GetTextElements("NoCyber")
                        ?.SymbolsByName
                        ?.GetValueOrDefault("NoCyber") is TextElements.Symbol noCyber)
                    output += $" {noCyber}";

                return output;
            }
        }

        public string Anatomy;
        public BodyPlanEntry Entry => BodyPlanFactory.Factory?.GetBodyPlanEntry(Anatomy);

        public string DisplayName => GetDescription();

        public List<TextElements> TextElements => Entry?.TextElements;

        protected static StringBuilder SB = new();

        protected static GameObject SampleCreature = null;

        public BodyPlan()
        {

        }

        public IEnumerable<string> GetDescriptionBefores(Predicate<TextElements> Where = null)
            => TextElements?.GetDescriptionBefores(Where)
            ;

        public IEnumerable<string> GetDescriptionAfters(Predicate<TextElements> Where = null)
            => TextElements?.GetDescriptionAfters(Where)
            ;

        public IEnumerable<string> GetSummaryBefores(Predicate<TextElements> Where = null)
            => TextElements?.GetSummaryBefores(Where)
            ;

        public IEnumerable<string> GetSummaryAfters(Predicate<TextElements> Where = null)
            => TextElements?.GetSummaryAfters(Where)
            ;

        public IEnumerable<string> GetSymbols(
            Predicate<TextElements> Where = null,
            Predicate<TextElements.Symbol> Filter = null
            )
            => TextElements?.GetSymbols(Where, Filter)
            ;

        private static string ColorBlack(string String)
            => "{{K|" + String + "}}"
            ;
        private static LimbTreeBranch InitializeLimbTreeBranch(BodyPart BodyPart)
            => BodyPart.InitializeLimbTreeBranch()
            ;
        public IEnumerable<string> GetBodyLimbTree(ref GameObject SampleCreature)
        {
            SampleCreature ??= GameObject.CreateSample("Humanoid");
            Entry.Anatomy.ApplyTo(SampleCreature.Body);
            return SampleCreature.Body.GetLimbTree(
                        IndentProc: ColorBlack,
                        BodyPartProc: InitializeLimbTreeBranch,
                        Treat0DepthPartsAsRoot: true)
                    .Select(l => l.ToString());
        }

        public IEnumerable<string> GetDescriptionLines()
        {
            bool newline = false;
            foreach (string descriptionBefore in GetDescriptionBefores())
            {
                newline = true;
                yield return descriptionBefore;
            }

            if (newline)
            {
                newline = false;
                yield return "";
            }

            bool didLimbs = false;
            foreach (var limbTreeBranch in GetBodyLimbTree(ref SampleCreature))
            {
                didLimbs = true;
                newline = true;
                yield return limbTreeBranch.ToString();
            }

            if (newline)
            {
                newline = false;
                yield return "";
            }

            if (didLimbs
                && SampleCreature.Body.GetFirstPart(bp => !bp.VariantTypeModel().DefaultBehavior.IsNullOrEmpty()) != null)
            {
                newline = true;
                yield return "{{W|Has natural equipment}}";
            }

            if (newline)
            {
                newline = false;
                yield return "";
            }

            foreach (string descriptionAfter in GetDescriptionAfters())
            {
                newline = true;
                yield return descriptionAfter;
            }
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
            GameObjectBlueprint DefaultBehvaiour,
            bool ExcludeName = false
            )
        {
            var sampleDefaultBehaviour = DefaultBehvaiour.createSample();

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
        public static string GetDefaultBehaviorString(string DefaultBehvaiour)
        {
            if (GameObject.CreateSample(DefaultBehvaiour) is not GameObject sampleDefaultBehaviour)
                return null;

            var mw = sampleDefaultBehaviour.GetPart<MeleeWeapon>();
            bool mwNotImprovisedAndNull = !(mw?.IsImprovisedWeapon() ?? true);
            string damage = mwNotImprovisedAndNull ? mw?.BaseDamage : null;

            int pVCap = mwNotImprovisedAndNull ? mw?.MaxStrengthBonus ?? 0 : 0;
            int pV = mwNotImprovisedAndNull ? 4 : 0;
            string pVSymbolColor = GetDisplayNamePenetrationColorEvent.GetFor(sampleDefaultBehaviour);

            var armor = sampleDefaultBehaviour.GetPart<Armor>();
            int aV = armor != null ? armor.AV : 0;
            int dV = armor != null ? armor.DV : 0;

            var sB = Event.NewStringBuilder();

            if (armor != null)
                sB.Append(' ').AppendArmor("y", aV, dV);

            if (pV > 0
                && pVCap > 0)
                sB.Append(' ').AppendPV(pVSymbolColor, "y", pV, pVCap);

            if (!damage.IsNullOrEmpty())
                sB.Append(' ').AppendDamage("y", damage);

            sampleDefaultBehaviour?.Obliterate();
            return Event.FinalizeString(sB);
        }
    }
}
