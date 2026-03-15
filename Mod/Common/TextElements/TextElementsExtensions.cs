using System;
using System.Collections.Generic;
using System.Text;

using XRL;
using XRL.Collections;
using XRL.World;

namespace UD_ChooseYourBodyPlan.Mod
{
    public static class TextElementsExtensions
    {
        public static IEnumerable<KeyValuePair<string, string>> GetTextElementsTags(this GameObjectBlueprint DataBucket)
        {
            string startsWith = $"{nameof(TextElements)}.";
            int startsWithIndex = startsWith.Length - 1;
            foreach ((var tagName, var tagValue) in DataBucket.GetTagsStartingWith(startsWith))
                yield return new(tagName[startsWithIndex..], tagValue);
        }

        public static IEnumerable<TextElements> GetTextElements(this IEnumerable<TextElements> TextElements, Predicate<TextElements> Where)
        {
            foreach (var textElements in TextElements)
                if (Where?.Invoke(textElements) is not false)
                    yield return textElements;
        }

        public static IEnumerable<string> GetDescriptionBefores(this IEnumerable<TextElements> TextElements, Predicate<TextElements> Where)
        {
            foreach (var textElements in TextElements.GetTextElements(Where))
                yield return textElements.DescriptionBefore;
        }

        public static IEnumerable<string> GetDescriptionAfters(this IEnumerable<TextElements> TextElements, Predicate<TextElements> Where)
        {
            foreach (var textElements in TextElements.GetTextElements(Where))
                yield return textElements.DescriptionAfter;
        }

        public static IEnumerable<string> GetSummaryBefores(this IEnumerable<TextElements> TextElements, Predicate<TextElements> Where)
        {
            foreach (var textElements in TextElements.GetTextElements(Where))
                yield return textElements.SummaryBefore;
        }

        public static IEnumerable<string> GetSummaryAfters(this IEnumerable<TextElements> TextElements, Predicate<TextElements> Where)
        {
            foreach (var textElements in TextElements.GetTextElements(Where))
                yield return textElements.SummaryAfter;
        }

        public static IEnumerable<string> GetSymbols(this IEnumerable<TextElements> TextElements, Predicate<TextElements> Where, Predicate<TextElements.Symbol> Filter)
        {
            foreach (var textElements in TextElements.GetTextElements(Where))
            {
                if (!textElements.Symbols.IsNullOrEmpty())
                {
                    foreach (var symbol in textElements.Symbols)
                        if (Filter?.Invoke(symbol) is not false)
                            yield return symbol.ToString();
                }
            }
        }
    }
}
