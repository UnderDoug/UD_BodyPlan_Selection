using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRL.Collections;
using XRL.World;

namespace UD_ChooseYourBodyPlan.Mod
{
    public static class OptionDelegateExtensions
    {
        public static bool CheckAll(this IEnumerable<OptionDelegate> OptionDelegates)
        {
            foreach (var option in OptionDelegates)
                if (!option.Check())
                    return false;
            return true;
        }

        public static bool CheckAny(this IEnumerable<OptionDelegate> OptionDelegates)
        {
            foreach (var option in OptionDelegates)
                if (option.Check())
                    return true;
            return false;
        }

        public static IEnumerable<bool> GetChecks(this IEnumerable<OptionDelegate> OptionDelegates)
        {
            foreach (var option in OptionDelegates)
                yield return option.Check();
        }

        public static bool Contains(
            this IEnumerable<OptionDelegate> OptionDelegates,
            string OptionID
            )
        {
            if (OptionDelegates.IsNullOrEmpty())
                return false;

            foreach (var optionDelegate in OptionDelegates)
                if (optionDelegate.OptionID == OptionID
                    && optionDelegate.IsValid())
                    return true;

            return false;
        }

        private static bool CheckProceed(
            IEnumerable<OptionDelegate> OptionDelegates,
            string OptionID
            )
        {
            if (OptionDelegates == null)
                return false;

            if (!OptionID.IsOption())
                return false;

            return true;
        }

        public static IEnumerable<OptionDelegate> GetWhere(
            this IEnumerable<OptionDelegate> OptionDelegates,
            Predicate<OptionDelegate> Where)
        {
            if (OptionDelegates.IsNullOrEmpty())
                yield break;

            foreach (var optionDelegate in OptionDelegates)
                if (Where?.Invoke(optionDelegate) is not false)
                    yield return optionDelegate;
        }

        public static bool ParseOptionTag(
            this ICollection<OptionDelegate> OptionDelegates,
            KeyValuePair<string, string> OptionTag
            )
        {
            if (OptionDelegates.IsNullOrEmpty())
                return false;

            string tagName = OptionTag.Key;
            string tagValue = OptionTag.Value;

            string optionID = null;
            string operatorString = null;
            string trueWhen = null;

            var validTags = OptionDelegate.ValidTags;

            if (validTags.Any(s => tagName == s))
            {
                optionID = tagValue;
                operatorString = null;
                trueWhen = null;
            }
            else
            if (tagName.Contains("."))
            {
                bool startsWithAny = validTags.Any(s => tagName.StartsWith($"{s}."));
                if (startsWithAny)
                {
                    if (tagName.Split(".") is string[] nameParams)
                    {
                        if (nameParams[1].IsOption())
                        {
                            optionID = nameParams[1];
                            operatorString = null;
                            trueWhen = tagValue;
                        }
                        else
                        if (tagName.IsOption())
                        {
                            if (nameParams[1].EqualsNoCase("remove"))
                            {
                                optionID = tagName;
                                operatorString = null;
                                trueWhen = Const.REMOVE_TAG;
                            }
                            else
                            if (nameParams[1].EqualsNoCase("require")
                                && !OptionDelegates.Contains(tagName))
                            {
                                optionID = tagName;
                                operatorString = null;
                                trueWhen = null;
                            }
                        }
                        else
                        if (nameParams[1].EqualsNoCase("require")
                            && OptionDelegate.TryParseOptionPredicate(tagName, out optionID, out operatorString, out trueWhen)
                            && OptionDelegates.Contains(optionID))
                        {
                            optionID = null;
                            operatorString = null;
                            trueWhen = null;
                        }
                    }
                }
            }
            else
            if (optionID.IsNullOrEmpty())
                Utils.ThisMod.Error($"{new ArgumentException($"Failed to parse into valid {nameof(OptionDelegate)}", nameof(OptionTag))}");

            return !optionID.IsNullOrEmpty()
                && OptionDelegates.Merge(optionID, operatorString, trueWhen);
        }

        public static IEnumerable<KeyValuePair<string, string>> GetOptionTags(this GameObjectBlueprint DataBucket)
            => DataBucket.GetTagsStartingWith("Option");

        public static bool ParseDataBucket(
            this ICollection<OptionDelegate> OptionDelegates,
            GameObjectBlueprint DataBucket
            )
        {
            if (DataBucket.GetOptionTags() is not IEnumerable<KeyValuePair<string, string>> tags
                || tags.IsNullOrEmpty())
                return true;

            bool any = false;
            foreach (var optionTag in tags)
                any = OptionDelegates.ParseOptionTag(optionTag) || any;

            return any;
        }

        public static OptionDelegate GetOptionDelegate(
            this ICollection<OptionDelegate> OptionDelegates,
            Predicate<OptionDelegate> Where
            )
        {
            if (OptionDelegates.IsNullOrEmpty())
                return null;

            if (Where == null)
                OptionDelegates.First();

            return OptionDelegates.FirstOrDefault(Where.Invoke);
        }

        public static OptionDelegate GetOptionDelegate(
            this ICollection<OptionDelegate> OptionDelegates,
            string OptionID
            )
            => CheckProceed(OptionDelegates, OptionID)
            ? OptionDelegates.GetOptionDelegate(o => o.OptionID == OptionID)
            : null
            ;

        public static bool TryGetOption(
            this ICollection<OptionDelegate> OptionDelegates,
            string OptionID,
            out OptionDelegate OptionDelegate
            )
            => (OptionDelegate = OptionDelegates.GetOptionDelegate(OptionID)) != null;

        public static bool Merge(
            this ICollection<OptionDelegate> OptionDelegates,
            string OptionID,
            string Operator,
            string TrueWhen
            )
        {
            if (CheckProceed(OptionDelegates, OptionID))
                return false;

            if (!OptionDelegates.TryGetOption(OptionID, out var existingOption))
            {
                if (!TrueWhen.IsNullOrEmpty()
                    && new OptionDelegate(OptionID, Operator, TrueWhen) is OptionDelegate newOption
                    && newOption.IsValid())
                {
                    OptionDelegates.Add(newOption);
                    return true;
                }
                return false;
            }

            if ((TrueWhen.IsNullOrEmpty()
                    || TrueWhen.EqualsNoCase(Const.REMOVE_TAG))
                && OptionDelegates.RemoveOptionID(OptionID))
                return true;

            if (!existingOption.ModifyTruth(Operator, TrueWhen).IsValid())
            {
                OptionDelegates.Remove(existingOption);
                return false;
            }

            return true;
        }

        public static bool Merge(
            this ICollection<OptionDelegate> OptionDelegates,
            OptionDelegate Source
            )
            => OptionDelegates.Merge(
                OptionID: Source.OptionID,
                Operator: Source.Operator,
                TrueWhen: Source.TrueWhen);

        public static bool RemoveOptionID(
            this ICollection<OptionDelegate> OptionDelegates,
            string OptionID
            )
        {
            if (!CheckProceed(OptionDelegates, OptionID))
                return false;

            bool any = false;
            using var iterator = ScopeDisposedList<OptionDelegate>.GetFromPoolFilledWith(OptionDelegates);
            foreach (var option in iterator)
            {
                if (option.OptionID == OptionID)
                {
                    OptionDelegates.Remove(option);
                    any = true;
                }
            }
            return any;
        }
    }
}
