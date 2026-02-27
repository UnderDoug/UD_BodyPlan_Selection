using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ConsoleLib.Console;

using XRL.UI.Framework;
using XRL.World;
using XRL.World.Anatomy;
using XRL.World.Parts;

namespace XRL.CharacterBuilds.Qud
{
    public class Qud_UD_BodyPlanModule : QudEmbarkBuilderModule<Qud_UD_BodyPlanModuleData>
    {
        public struct AnatomyChoice
        {
            public Anatomy Anatomy;

            private Renderable Renderable;

            public string GetDescription()
                => sb
                    .Clear()
                    .Append(Anatomy.Name)
                    .ToString();

            public string GetLongDescription()
                => sb
                    .Clear()
                    .Append(Anatomy.Name)
                    .ToString();

            public Renderable GetRenderable()
            {
                if (Renderable == null
                    && Anatomy != null)
                    Renderable = GetExampleBlueprint()?.GetRenderable();

                return Renderable;
            }

            public static bool HasMatchingAnatomy(GameObjectBlueprint Blueprint, Anatomy Anatomy)
                => Blueprint != null
                && Blueprint.TryGetPartParameter(nameof(Body), nameof(Body.Anatomy), out string anatomy) && anatomy == Anatomy.Name
                ;
            public bool HasMatchingAnatomy(GameObjectBlueprint Blueprint)
                => HasMatchingAnatomy(Blueprint, Anatomy);

            public IEnumerable<GameObjectBlueprint> GetExampleBlueprints()
                => GameObjectFactory.Factory
                    ?.BlueprintList
                    ?.Where(HasMatchingAnatomy)
                    ?.Where(b => !b.IsBaseBlueprint())
                ;

            public GameObjectBlueprint GetExampleBlueprint()
                => GetExampleBlueprints()?.GetRandomElementCosmetic()
                ?? GameObjectFactory.Factory.GetBlueprintIfExists("Bep");
        }

        public override AbstractEmbarkBuilderModuleData DefaultData => GetDefaultData();

        public List<AnatomyChoice> AnatomyChoices = new();

        protected static StringBuilder sb = new();

        public override void InitFromSeed(string seed)
        { }

        public override bool shouldBeEditable()
            => builder.IsEditableGameMode();

        public override bool shouldBeEnabled()
            => (builder.GetModule<QudGenotypeModule>()?.data?.Entry?.IsMutant ?? false)
            && builder?.GetModule<QudSubtypeModule>()?.data?.Subtype != null;

        public override SummaryBlockData GetSummaryBlock()
        {
            AnatomyChoice anatomyChoice = SelectedChoice();
            anatomyChoice.GetRenderable();
            return new SummaryBlockData
            {
                Id = GetType().FullName,
                Title = "Body Plan",
                Description = anatomyChoice.GetDescription(),
                SortOrder = 100
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
                    || data.Selections.IsNullOrEmpty())
                {
                    MetricsManager.LogWarning("Body Plan module was active but data or selections was null or empty.");
                    return element;
                }
                if (element is GameObject player
                    && player.Body is Body playerBody)
                    playerBody.Rebuild(data.Selections[0].Anatomy);
            }
            return base.handleBootEvent(id, game, info, element);
        }

        public override string DataErrors()
        {
            if (data != null
                && !AnatomyChoices.Any(IsSelected))
                return "Invalid choice selected";

            return base.DataErrors();
        }
        public override void handleModuleDataChange(
            AbstractEmbarkBuilderModule module,
            AbstractEmbarkBuilderModuleData oldValues,
            AbstractEmbarkBuilderModuleData newValues
            )
        {
            if (module is not QudSubtypeModule && module is not QudGenotypeModule)
                return;

            AnatomyChoices.Clear();
            AnatomyChoices.AddRange(Anatomies.AnatomyList.Select(a => new AnatomyChoice() { Anatomy = a }));
        }

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
            => new(GetDefaultPlayerBodyPlan());

        public bool IsSelected(AnatomyChoice Choice)
        {
            if (data == null)
                return false;

            if (data.Selections.Count == 0)
                return Choice.Anatomy == null;

            return data.Selections[0] is Qud_UD_BodyPlanModuleDataRow bodyPlanDataRow
                && bodyPlanDataRow.Anatomy == Choice.Anatomy?.Name;
        }

        public AnatomyChoice SelectedChoice()
            => AnatomyChoices.Find(IsSelected);
    }
}
