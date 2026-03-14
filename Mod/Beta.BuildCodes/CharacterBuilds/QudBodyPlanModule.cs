using XRL.CharacterBuilds;

namespace UD_ChooseYourBodyPlan.Mod.CharacterBuilds
{
    public partial class QudBodyPlanModule : QudEmbarkBuilderModule<QudBodyPlanModuleData>
    {
        public override string GetRequiredMod()
            => SelectedChoice() != PlayerAnatomyChoice
            ? $"{Utils.ThisMod.DisplayTitle} (Anatomy: {SelectedChoice()?.Anatomy?.Name ?? BodyPlanEntry.MISSING_ANATOMY})"
            : null;
    }
}
