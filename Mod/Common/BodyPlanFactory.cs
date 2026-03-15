using System;
using System.Collections.Generic;
using System.Text;

using XRL;
using XRL.UI;
using XRL.World;
using XRL.World.Anatomy;

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

        public Dictionary<string, TextElements> TextElementsByName;

        public Dictionary<string, TransformationData> TransformationDataByAnatomyName;

        public Dictionary<string, BodyPlanEntry> BodyPlanEntryByAnatomyName;

        public Dictionary<string, AnatomyCategory> AnatomyCategoryByCategoryName;

        public bool TextElementsInitialized;

        public bool TransformationDataInitialized;

        public BodyPlanFactory()
        {
            TextElementsInitialized = false;
            TransformationDataInitialized = false;
        }

        public void LoadTextElements()
        {
            Load(ref TextElementsByName);
            TextElementsInitialized = true;
        }

        public void LoadTransformationData()
        {
            Load(ref TransformationDataByAnatomyName);
            TransformationDataInitialized = true;
        }

        public void LoadBodyPlans()
        {

            Load(ref BodyPlanEntryByAnatomyName);
        }

        public void LoadBodyPlanCategories()
        {
            Load(ref AnatomyCategoryByCategoryName);
        }

        public void Load<T>(ref Dictionary<string, T> CacheByName)
            where T : ILoadFromDataBucket<T>, new()
        {
            CacheByName = new();
            foreach (var dataBucket in GetDataBuckets<T>())
            {
                if (TryLoadFromDataBucket(dataBucket, out T loaded))
                {
                    if (CacheByName.ContainsKey(loaded.CacheKey))
                        CacheByName[loaded.CacheKey].Merge(loaded);
                    else
                        CacheByName[loaded.CacheKey] = loaded;
                }
            }
        }

        public IEnumerable<GameObjectBlueprint> GetDataBuckets<T>()
            where T : ILoadFromDataBucket<T>, new()
            => GameObjectFactory.Factory
                ?.GetBlueprintsInheritingFrom(ILoadFromDataBucket<TextElements>.GetBaseDataBucketBlueprint())
            ;

        public T LoadFromDataBucket<T>(GameObjectBlueprint DataBucket)
            where T : ILoadFromDataBucket<T>, new()
            => new T().LoadFromDataBucket(DataBucket);

        public bool TryLoadFromDataBucket<T>(GameObjectBlueprint DataBucket, out T Result)
            where T : ILoadFromDataBucket<T>, new()
            => (Result = LoadFromDataBucket<T>(DataBucket)) != null;

        public TransformationData GetTransformationData(string AnatomyName)
            => !AnatomyName.IsNullOrEmpty()
            ? TransformationDataByAnatomyName.GetValueOrDefault(AnatomyName)
            : null
            ;

        public TransformationData GetTransformationData(Anatomy Anatomy)
            => GetTransformationData(Anatomy?.Name)
            ;

        public TransformationData GetTransformationData(BodyPlanEntry BodyPlanEntry)
            => GetTransformationData(BodyPlanEntry?.Anatomy)
            ;
    }
}
