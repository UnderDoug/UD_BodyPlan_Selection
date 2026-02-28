using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ConsoleLib.Console;

using UD_BodyPlan_Selection.Mod;

using XRL.CharacterBuilds.Qud.UI;
using XRL.Collections;
using XRL.UI.Framework;
using XRL.World;
using XRL.World.Anatomy;
using XRL.World.Parts;

namespace XRL.CharacterBuilds.Qud
{
    public class Qud_UD_BodyPlanModule : QudEmbarkBuilderModule<Qud_UD_BodyPlanModuleData>
    {
        public class AnatomyChoice
        {
            public static string MISSING_ANATOMY => nameof(MISSING_ANATOMY);

            public Anatomy Anatomy;

            private Renderable Renderable;

            public bool IsDefault;

            public string GetDescription()
            {
                SB.Clear();

                SB.Append(Anatomy?.Name.SplitCamelCase() ?? MISSING_ANATOMY);
                if (IsDefault)
                    SB.Append(" (default)");

                return SB.ToString();
            }

            public string GetLongDescription(bool IncludeOpening = false)
            {
                SB.Clear();

                var limbCounts = new Dictionary<BodyPartType, int>();
                if (Anatomy?.Parts.Select(a => a.Type) is IEnumerable<BodyPartType> limbs)
                    foreach (BodyPartType limb in limbs)
                    {
                        if (limbCounts.ContainsKey(limb))
                            limbCounts[limb]++;
                        else
                            limbCounts[limb] = 1;
                    }

                if (IncludeOpening)
                    SB.Append("Contains the following body parts:");

                foreach ((BodyPartType limb, int count) in limbCounts)
                {
                    if (!SB.IsNullOrEmpty())
                        SB.AppendLine();

                    string timesColored = "}}x {{W|";
                    string limbName = timesColored + limb.Name;
                    string limbPluralName = null;
                    if (limb.Plural.GetValueOrDefault())
                        limbPluralName = limbName;

                    SB.Append("{{W|").Append(count.Things(limbName, limbPluralName)).Append("}}");
                    if (!limb.Name.EqualsNoCase(limb.FinalType))
                        SB.Append(" (").Append(limb.FinalType).Append(")");
                }
                return SB.ToString();
            }

            public Renderable GetRenderable()
            {
                if (Renderable == null
                    && Anatomy != null)
                    Renderable = GetExampleBlueprint()?.GetRenderable();

                return Renderable;
            }

            private static string GetTile(GameObjectBlueprint Blueprint)
                => Blueprint.GetPartParameter<string>(nameof(Render), nameof(Render.Tile))
                ;
            private static string GetAnatomy(GameObjectBlueprint Blueprint)
                => Blueprint.GetPartParameter<string>(nameof(Body), nameof(Body.Anatomy))
                ;
            public bool IsEligibleForDisplay(GameObjectBlueprint Blueprint)
                => Blueprint != null
                && (!Blueprint.IsBaseBlueprint()
                    || GetTile(Blueprint) != null)
                && !Blueprint.HasTag("Golem")
                && ((GetTile(Blueprint) is string renderTile
                        && !renderTile.Contains("sw_farmer"))
                    || (GetAnatomy(Blueprint)?.EqualsNoCase("Humanoid") ?? false))
                && !Blueprint.Name.Contains("Cherub")
                ;
            public bool HasMatchingAnatomy(GameObjectBlueprint Blueprint)
                => Blueprint != null
                && GetAnatomy(Blueprint) is string anatomy
                && anatomy == Anatomy.Name
                ;
            public bool ObjectAnimatesWithAnatomy(GameObjectBlueprint Blueprint)
                => Blueprint != null
                && Blueprint.TryGetTag("BodyType", out string bodyType) 
                && bodyType == Anatomy.Name
                ;
            public bool InheritsFromAnatomy(GameObjectBlueprint Blueprint)
                => Blueprint != null
                && Blueprint.InheritsFrom(Anatomy.Name)
                ;

            public IEnumerable<GameObjectBlueprint> GetExampleBlueprints()
            {
                if (GameObjectFactory.Factory
                    ?.BlueprintList
                    ?.Where(IsEligibleForDisplay) 
                    ?.Where(HasMatchingAnatomy) is IEnumerable<GameObjectBlueprint> objectsWithAnatomy
                    && !objectsWithAnatomy.IsNullOrEmpty())
                    return objectsWithAnatomy;

                if (GameObjectFactory.Factory
                    ?.BlueprintList
                    ?.Where(IsEligibleForDisplay)
                    ?.Where(ObjectAnimatesWithAnatomy) is IEnumerable<GameObjectBlueprint> objectsAnimatingWithAnatomy
                    && !objectsAnimatingWithAnatomy.IsNullOrEmpty())
                    return objectsAnimatingWithAnatomy;

                if (GameObjectFactory.Factory
                    ?.BlueprintList
                    ?.Where(IsEligibleForDisplay)
                    ?.Where(InheritsFromAnatomy) is IEnumerable<GameObjectBlueprint> objectsInheritingAnatomy
                    && !objectsInheritingAnatomy.IsNullOrEmpty())
                    return objectsInheritingAnatomy;

                return new GameObjectBlueprint[0];
            }

            public GameObjectBlueprint GetExampleBlueprint()
                => GetExampleBlueprints()?.GetRandomElementCosmetic()
                ?? GameObjectFactory.Factory.GetBlueprintIfExists("Mimic")
                ;
        }

        public static bool IsEligibleAnatomy(Anatomy Anatomy)
            => Anatomy != null
            && Anatomy.BodyCategory != BodyPartCategory.LIGHT
            && Anatomy.Category != BodyPartCategory.MECHANICAL
            && Anatomy.Name != "HumanoidWithHandsFace"
            && Anatomy.Name != "Echinoid"
            // && true.LogReturning(Anatomy.Name)
            ;
        public static AnatomyChoice AnatomyToChoice(Anatomy Anatomy)
            => new () { Anatomy = Anatomy }
            ;
        public static IEnumerable<AnatomyChoice> BaseAnatomyChoices => Anatomies.AnatomyList
            ?.Where(IsEligibleAnatomy)
            ?.Select(AnatomyToChoice)
            ?.OrderBy(a => a.Anatomy.Name)
            ;

        private AnatomyChoice _PlayerAnatomyChoice; 
        public AnatomyChoice PlayerAnatomyChoice
        {
            get
            {
                if (_PlayerAnatomyChoice == null
                    && GetDefaultPlayerBodyPlan() is string playerAnatomyName)
                    _PlayerAnatomyChoice = AnatomyChoices.FirstOrDefault(a => a?.Anatomy?.Name == playerAnatomyName);

                return _PlayerAnatomyChoice;
            }
        }

        public override AbstractEmbarkBuilderModuleData DefaultData => GetDefaultData();

        private List<AnatomyChoice> _AnatomyChoices;
        public List<AnatomyChoice> AnatomyChoices
        {
            get
            {
                if (_AnatomyChoices.IsNullOrEmpty())
                {
                    _AnatomyChoices ??= new();
                    _AnatomyChoices.AddRange(BaseAnatomyChoices.Where(a => a.GetDescription() != AnatomyChoice.MISSING_ANATOMY));
                    _AnatomyChoices.RemoveAll(c => c == null);
                    SetDefautChoice();
                }
                return _AnatomyChoices;
            }
        }

        protected static StringBuilder SB = new();

        public Qud_UD_BodyPlanModule()
        {
            _AnatomyChoices = null;
            _PlayerAnatomyChoice = null;
        }

        public override void InitFromSeed(string seed)
        { }

        public override bool shouldBeEditable()
            => builder.IsEditableGameMode();

        public override bool shouldBeEnabled()
            => builder.GetModule<QudGenotypeModule>()?.data?.Entry is GenotypeEntry genotypeEntry
            && !genotypeEntry.IsTrueKin
            && builder?.GetModule<QudSubtypeModule>()?.data?.Subtype != null;

        private static bool IsQudQudMutationsModuleWindowDescriptor(EmbarkBuilderModuleWindowDescriptor Descriptor)
            => Descriptor.viewID == "Chargen/Mutations"
            ;
        public override void assembleWindowDescriptors(List<EmbarkBuilderModuleWindowDescriptor> windows)
        {
            /*
            foreach (EmbarkBuilderModuleWindowDescriptor descriptor in windows)
                UnityEngine.Debug.Log(descriptor?.viewID ?? "No viewID");
            */
            int index = windows.FindIndex(IsQudQudMutationsModuleWindowDescriptor);
            if (index < 0)
                base.assembleWindowDescriptors(windows);
            else
                windows.InsertRange(index + 1, this.windows.Values);
        }

        public override SummaryBlockData GetSummaryBlock()
        {
            AnatomyChoice anatomyChoice = SelectedChoice();
            anatomyChoice.GetRenderable();
            return new SummaryBlockData
            {
                Id = GetType().FullName,
                Title = "Body Plan",
                Description = anatomyChoice.GetDescription() + "\n" + anatomyChoice.GetLongDescription(),
                SortOrder = 50
            };
        }

        public override object handleBootEvent(
            string id,
            XRLGame game,
            EmbarkInfo info,
            object element = null
            )
        {
            if (id == QudGameBootModule.BOOTEVENT_BOOTPLAYEROBJECT)
            {
                if (data == null
                    || data.Selection == null)
                {
                    MetricsManager.LogWarning("Body Plan module was active but data or selections was null or empty.");
                    return element;
                }
                if (element is GameObject player
                    && player.Body is Body playerBody)
                    playerBody.Rebuild(data.Selection.Anatomy);
            }
            return base.handleBootEvent(id, game, info, element);
        }

        public override string DataErrors()
        {
            if (!AnatomyChoices.Any(IsSelected))
                return "Invalid choice selected";

            return base.DataErrors();
        }
        public override void handleModuleDataChange(
            AbstractEmbarkBuilderModule module,
            AbstractEmbarkBuilderModuleData oldValues,
            AbstractEmbarkBuilderModuleData newValues
            )
        {
            if (module is not QudGenotypeModule genotypeModule
                && module is not QudSubtypeModule)
                return;

            if (module == this)
                return;

            OrganizeAnatomyChoices();
        }

        public void OrganizeAnatomyChoices(bool SelectDefaultChoice = false)
        {
            _PlayerAnatomyChoice = null;
            if (PlayerAnatomyChoice != null
                && AnatomyChoices[0] != PlayerAnatomyChoice)
            {
                AnatomyChoices.OrderBy(a => a.Anatomy.Name);
                AnatomyChoices.Remove(PlayerAnatomyChoice);
                AnatomyChoices.Insert(0, PlayerAnatomyChoice);
                SetDefautChoice(SelectDefaultChoice);
            }
        }

        public void PickAnatomy(int n)
            => setData(new Qud_UD_BodyPlanModuleData(AnatomyChoices[n]));

        private string GetPlayerBlueprint()
        {
            var body = builder.GetModule<QudGenotypeModule>()?.data?.Entry?.BodyObject
                .Coalesce(builder.GetModule<QudSubtypeModule>()?.data?.Entry?.BodyObject)
                .Coalesce("Humanoid");

            return builder.info?.fireBootEvent(QudGameBootModule.BOOTEVENT_BOOTPLAYEROBJECTBLUEPRINT, The.Game, body);
        }

        public string GetDefaultPlayerBodyPlan()
            => GameObjectFactory.Factory
                ?.GetBlueprintIfExists(GetPlayerBlueprint())
                ?.GetPartParameter<string>(nameof(Body), nameof(Body.Anatomy));

        public Qud_UD_BodyPlanModuleData GetDefaultData()
            => new(PlayerAnatomyChoice);

        public void SetDefautChoice(bool SelectDefaultChoice = false)
        {
            if (PlayerAnatomyChoice is AnatomyChoice defaultChoice)
            {
                defaultChoice.IsDefault = true;

                if (SelectDefaultChoice
                    && (data.Selection == null
                        || data.Selection.Anatomy.IsNullOrEmpty()))
                    data.Selection = new(defaultChoice);
            }
        }

        public bool IsSelected(AnatomyChoice Choice)
            => Choice != null
            && data != null
            && ((Choice.Anatomy == null
                    && data.Selection == null)
                || Choice.Anatomy?.Name == data.Selection.Anatomy);

        public AnatomyChoice SelectedChoice()
            => AnatomyChoices.Find(IsSelected);
    }
}
