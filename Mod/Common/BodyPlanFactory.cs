using System;
using System.Collections.Generic;
using System.Text;

using XRL;
using XRL.UI;
using XRL.World;

namespace UD_ChooseYourBodyPlan.Mod
{
    [HasModSensitiveStaticCache]
    public class BodyPlanFactory
    {
        [ModSensitiveStaticCache]
        private static BodyPlanFactory _Factory;
        public static BodyPlanFactory Factory
        {
            get
            {
                if (_Factory == null)
                {
                    _Factory = new();
                    Loading.LoadTask("Loading TransformationData.xml", _Factory.LoadTransformationData);
                    Loading.LoadTask("Loading BodyPlans.xml", _Factory.LoadBodyPlans);
                    Loading.LoadTask("Loading BodyPlanCategories.xml", _Factory.LoadBodyPlanCategories);
                    Loading.LoadTask("Loading TextElements.xml", _Factory.LoadTextElements);
                }
                return _Factory;
            }
        }

        public static Dictionary<string, TransformationData> TransformationDataByAnatomyName;

        public static Dictionary<string, BodyPlanEntry> BodyPlanEntryByAnatomyName;

        public static Dictionary<string, AnatomyCategory> AnatomyCategoryByCategoryName;

        public static Dictionary<string, TextElements> TextElementsByName;

        public void LoadTransformationData()
        {

        }

        public void LoadBodyPlans()
        {

        }

        public void LoadBodyPlanCategories()
        {

        }

        public void LoadTextElements()
        {

        }

        public ILoadFromDataBucket<T> LoadFromDataBucket<T>(GameObjectBlueprint DataBucket)
            where T : ILoadFromDataBucket<T>, new()
            => new T().LoadFromDataBucket(DataBucket);
    }
}
