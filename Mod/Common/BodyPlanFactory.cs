using System;
using System.Collections.Generic;
using System.Text;

using XRL;
using XRL.World;

namespace UD_BodyPlan_Selection.Mod
{
    [HasModSensitiveStaticCache]
    public static class BodyPlanFactory
    {
        [ModSensitiveStaticCache]
        private static bool Initialized = false;

        [ModSensitiveStaticCache]
        private static Dictionary<string, BodyPlanEntry> _EntriesByID;
        public static Dictionary<string, BodyPlanEntry> EntriesByID
        {
            get
            {
                if (_EntriesByID.IsNullOrEmpty())
                    LogCacheInitError(nameof(EntriesByID));

                return _EntriesByID;
            }
        }

        private static void LogError(object Message)
            => MetricsManager.LogCallingModError(Message);

        private static void LogCacheInitError(string CacheName)
        {
            string reason = Initialized
                ? "initialized incorrectly"
                : "not initialized";
            LogError($"{CacheName} empty or null, {nameof(BodyPlanFactory)} {reason}");
        }

        private static void LogException(string MethodName, Exception x)
            => LogError($"{nameof(BodyPlanFactory)}.{MethodName} {x}");

        [ModSensitiveCacheInit]
        public static void Init()
        {
            if (Initialized)
                MetricsManager.LogCallingModError($"{nameof(BodyPlanEntry)}.{nameof(Init)} called after already initialized");

            _EntriesByID = new();
            try
            {
                

                Initialized = true;
            }
            catch (Exception x)
            {
                Initialized = false;
                LogException(nameof(Init), x);
            }
        }

        public static void InitBodyPlanEntries()
        {
            try
            {
                if (GameObjectFactory.Factory?.GetBlueprintsInheritingFrom(CATEGORY_BASE_BLUEPRINT) is IEnumerable<GameObjectBlueprint> categoryDataBuckets)
                    foreach (var dataBucket in categoryDataBuckets)
                    {
                        if (dataBucket.xTags.IsNullOrEmpty()
                            || !dataBucket.xTags.ContainsKey(SchoolCategoryXTag)
                            || LoadCategoryFromDataBucket(dataBucket) is not SchoolCategory category
                            || !category.LogDebugString().IsValid())
                            continue;

                        _EntriesByID.Add(category);
                    }
            }
            catch (Exception x)
            {
                LogException(nameof(InitBodyPlanEntries), x);
            }
        }
    }
}
