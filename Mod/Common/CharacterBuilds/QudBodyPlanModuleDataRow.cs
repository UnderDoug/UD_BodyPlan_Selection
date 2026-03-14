using System;

using static UD_ChooseYourBodyPlan.Mod.AnatomyConfiguration;

namespace UD_ChooseYourBodyPlan.Mod.CharacterBuilds
{
    [Serializable]
    public class QudBodyPlanModuleDataRow
    {
        public string Anatomy;
        public TransformationData Transformation;

        public QudBodyPlanModuleDataRow()
        {
            Anatomy = null;
            Transformation = null;
        }
        public QudBodyPlanModuleDataRow(string Anatomy, TransformationData Transformation)
            : this()
        {
            this.Anatomy = Anatomy;
            this.Transformation = Transformation;
        }
        public QudBodyPlanModuleDataRow(BodyPlanEntry Choice)
            : this(Choice?.Anatomy?.Name, Choice?.AnatomyConfigurations?.FirstTransformationOrDefault())
        { }
    }
}
