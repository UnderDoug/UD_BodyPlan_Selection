using XRL.CharacterBuilds;
using XRL.World.Anatomy;

using static UD_ChooseYourBodyPlan.Mod.AnatomyConfiguration;

namespace UD_ChooseYourBodyPlan.Mod.CharacterBuilds
{
    public class QudBodyPlanModuleData : AbstractEmbarkBuilderModuleData
    {
        public QudBodyPlanModuleDataRow Selection;

        public bool HasSelection => Selection?.Anatomy != null;

        public QudBodyPlanModuleData()
        {
            Selection = null;
            Version = Utils.ThisMod.Manifest.Version;
        }

        public QudBodyPlanModuleData(string Selection, TransformationData Transformation)
            : this()
            => this.Selection = !Selection.IsNullOrEmpty()
                ? new QudBodyPlanModuleDataRow(Selection, Transformation)
                : null
            ;

        public QudBodyPlanModuleData(Anatomy Selection, TransformationData Transformation = null)
            : this(Selection?.Name, Transformation)
        { }

        public QudBodyPlanModuleData(BodyPlanEntry Selection)
            : this(Selection?.Anatomy, Selection?.AnatomyConfigurations?.FirstTransformationOrDefault())
        { }
    }
}
