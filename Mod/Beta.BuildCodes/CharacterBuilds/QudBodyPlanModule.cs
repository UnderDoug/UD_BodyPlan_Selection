using XRL.CharacterBuilds;

namespace UD_BodyPlan_Selection.Mod.CharacterBuilds
{
    public partial class QudBodyPlanModule : QudEmbarkBuilderModule<QudBodyPlanModuleData>
    {
        public override string GetRequiredMod()
            => SelectedChoice() != PlayerAnatomyChoice
            ? $"{Utils.ThisMod.DisplayTitle} (Anatomy: {SelectedChoice()?.Anatomy?.Name ?? AnatomyChoice.MISSING_ANATOMY})"
            : null;
    }
}
