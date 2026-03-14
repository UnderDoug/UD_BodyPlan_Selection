using XRL.UI.Framework;

using XRL.CharacterBuilds;

namespace UD_ChooseYourBodyPlan.Mod.CharacterBuilds.UI
{
    public partial class QudBodyPlanModuleWindow : EmbarkBuilderModuleWindowPrefabBase<QudBodyPlanModule, CategoryMenusScroller>
    {
        // Gets called in the main file, actually checks something in it's beta-branch counterpart.

        public bool SkippingUIUpdates()
            => false;
    }
}
