using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using XRL;
using XRL.Collections;
using XRL.World;
using XRL.World.Anatomy;
using XRL.World.Parts;

using static UD_BodyPlan_Selection.Mod.Utils;

namespace UD_BodyPlan_Selection.Mod
{
    public static class Extensions
    {
        public static Func<string, T> ToFunc<T>(this Parse<T> Parse)
            => Parse.Invoke;
        public static Parse<T> ToParse<T>(this Func<string, T> Func)
            => s => Func(s);

        public static string SplitCamelCase(this string String)
            => !String.Contains(" ")
            ? Regex.Replace(
                input: Regex.Replace(
                    input: String,
                    pattern: @"(\P{Ll})(\P{Ll}\p{Ll})",
                    replacement: "$1 $2"),
                pattern: @"(\p{Ll})(\P{Ll})",
                replacement: "$1 $2")
            : String
            ;

        public static bool LogReturning(this bool Return, string Message)
            => LogReturnBool(Return, Message);

        public static bool HasSTag(this GameObjectBlueprint Blueprint, string STag)
            => Blueprint?.Tags?.Keys is Dictionary<string, string>.KeyCollection keys
            && keys.Any(s => s.Equals("Semantic" + STag));

        public static bool InheritsFromAny(this GameObjectBlueprint Blueprint, params string[] Blueprints)
            => !Blueprints.IsNullOrEmpty()
            && Blueprints.Any(bp => Blueprint.InheritsFrom(bp));

        public static string ThisManyTimes(this string @string, int Times = 1)
            => Times.Aggregate("", (a, n) => a + @string)
            ;
        public static string ThisManyTimes(this char @char, int Times = 1)
            => @char.ToString().ThisManyTimes(Times)
            ;

        public static string CallChain(this string String, params string[] Calls)
            => Calls.Aggregate(String, (a, n) => a + "." + n)
            ;

        public static string CallChain(this Type Type, params string[] Calls)
            => Type.Name.CallChain(Calls);

        public static bool Sucks(this Anatomy Anatomy)
            => Anatomy.BodyCategory == BodyPartCategory.LIGHT
            || Anatomy.Category == BodyPartCategory.LIGHT
            || Anatomy.Name == "Echinoid"
            ;

        public static bool HasRecipe(this Anatomy Anatomy)
            => new string[]
            {
                "SlugWithHands",
                "HumanoidOctohedron",
            }
            .Contains(Anatomy.Name);

        public static StringBuilder AppendNoCybernetics(this StringBuilder SB, bool PrependSpace = true)
        {
            if (PrependSpace)
                SB.Append(' ');
            return SB.AppendColored("r", "\x009b");
        }
        public static StringBuilder AppendNaturalWeapon(this StringBuilder SB, bool PrependSpace = true)
        {
            if (PrependSpace)
                SB.Append(' ');
            return SB.AppendColored("w", "\x0006");
        }

        public static string GetTile(this GameObjectBlueprint Blueprint)
            => Utils.GetTile(Blueprint)
            ;
        public static string GetAnatomyName(this GameObjectBlueprint Blueprint)
            => Utils.GetAnatomyName(Blueprint)
            ;
        public static Anatomy GetAnatomy(this GameObjectBlueprint Blueprint)
            => Utils.GetAnatomy(Blueprint)
            ;

        public static T Coalesce<T>(this T Object, T OtherObject)
            => Object ?? OtherObject;

        public static TAccumulate Aggregate<TAccumulate>(
            this int Number,
            TAccumulate seed,
            Func<TAccumulate, int, TAccumulate> func
            )
        {
            for (int i = 0; i < Number; i++)
                seed = func(seed, i);

            return seed;
        }

        public static StringBuilder AppendLines(this StringBuilder SB, int Count)
            => Count.Aggregate(SB, (a, n) => a.AppendLine())
            ;

        public static StringBuilder AppendDamage(this StringBuilder SB, string Damage)
            => SB.AppendColored("r", "\u0003").Append(Damage)
            ;
        public static StringBuilder AppendDamage(this StringBuilder SB, string Color, string Damage)
            => SB.AppendColored("r", "\u0003").AppendColored(Color, Damage)
            ;

        public static StringBuilder AppendAV(this StringBuilder SB, int AV)
            => SB.AppendColored("b", "\u0004").Append(AV)
            ;
        public static StringBuilder AppendAV(this StringBuilder SB, string Color, int AV)
            => SB.AppendColored("b", "\u0004").AppendColored(Color, AV.ToString())
            ;

        public static StringBuilder AppendDV(this StringBuilder SB, int DV)
            => SB.AppendColored("K", "\t").Append(DV)
            ;
        public static StringBuilder AppendDV(this StringBuilder SB, string Color, int DV)
            => SB.AppendColored("K", "\t").AppendColored(Color, DV.ToString())
            ;

        public static StringBuilder AppendArmor(this StringBuilder SB, int AV, int DV)
            => SB.AppendAV(AV).Append(' ').AppendDV(DV)
            ;
        public static StringBuilder AppendArmor(this StringBuilder SB, string Color, int AV, int DV)
            => SB.AppendAV(Color, AV).Append(' ').AppendDV(Color, DV)
            ;

        public static bool EndsWithAny(this string String, params string[] Values)
            => Values.IsNullOrEmpty()
            || Values.Any(s => String.EndsWith(s));

        /// <summary>
        /// Writes a line to the stream with an optional indent, factored to 2.
        /// </summary>
        /// <param name="Writer">The <see cref="StreamWriter"/> object.</param>
        /// <param name="Value">The Value to write to the stream on its own line.</param>
        /// <param name="Indent">The level of indent (2 spaces) for this line.</param>
        /// <returns>The <see cref="StreamWriter"/> object.</returns>
        public static StreamWriter WriteLine2(this StreamWriter Writer, string Value, int Indent = 0)
        {
            if (Indent > 0)
                Value = " ".ThisManyTimes(Indent * 2) + Value;
            Writer.Write(Value + "\n");
            UnityEngine.Debug.Log(Value);
            return Writer;
        }

        public static StreamWriter WriteLine4(this StreamWriter Writer, string Value, int Indent = 0)
            => Writer.WriteLine2(Value, Indent * 2)
            ;

        /*
        /// <summary>
        /// 
        /// </summary>
        /// <param name="BodyPart">The body part from which to produce a limb tree.</param>
        /// <param name="Selector">The type parameters of this Func match the other arguments of this method: <paramref name="BodyPart"/>, <paramref name="Proc"/>, <paramref name="IndentDrawing"/>, <paramref name="DepthLimit"/>, <paramref name="Depth"/>, <paramref name="SiblingOrdinal"/>, <paramref name="SiblingCardinal"/>; returning one element of this method's return value.</param>
        /// <param name="Proc">Processing to be performed on <paramref name="BodyPart"/> to get a display value for it.</param>
        /// <param name="IndentDrawing">Records what should appear at each level of the indent.</param>
        /// <param name="DepthLimit">The maximum depth that indentation should go (truncates anything in excess).</param>
        /// <param name="Depth">The current depth of indentation.</param>
        /// <param name="SiblingOrdinal">The index of the <paramref name="BodyPart"/> in the parent collection of elements in which it might exist.</param>
        /// <param name="SiblingCardinal">The total number of elements in the parent collection in which the <paramref name="BodyPart"/> might exist.</param>
        /// <returns>A collection of strings representing the <paramref name="BodyPart"/> and any subparts it might have in a tree configuration.</returns>
        public static IEnumerable<string> GetLimbTreeLines(
            this BodyPart BodyPart,
            Func<BodyPart, Func<BodyPart, string>, Dictionary<int, char>, int, int, int, string> Selector,
            Func<BodyPart, string> Proc,
            Dictionary<int, char> IndentDrawing,
            int DepthLimit = int.MaxValue,
            int Depth = 0,
            int SiblingOrdinal = 1,
            int SiblingCardinal = 1
            )
        {
            IndentDrawing ??= new();
            if (BodyPart == null)
                yield break;

            yield return Selector(BodyPart, Proc, IndentDrawing, Depth, SiblingOrdinal, SiblingCardinal);
            if (Depth >= DepthLimit)
                yield break;

            if (BodyPart.LoopSubparts() is IEnumerable<BodyPart> subparts)
            {
                int children = subparts.Count();
                int child = 0;
                if (subparts.SelectMany(o => o.GetLimbTreeLines(Selector, Proc, IndentDrawing, DepthLimit, Depth + 1, ++child, children)) is IEnumerable<string> subResults)
                    foreach (string subResult in subResults)
                        yield return subResult;
            }
        }
        public static string GetLimbBranch(
            BodyPart BodyPart,
            Func<BodyPart, string> Proc,
            Dictionary<int, char> IndentDrawing,
            int Depth,
            int Ordinal,
            int Cardinal
            )
        {
            var indent = Event.NewStringBuilder();

            const char NBSP = '\u00ff'; // non-breaking space
            const char VERT = '\u00b3'; // vertical
            const char UANR = '\u00c0'; // up and right
            const char VERR = '\u00c3'; // vertical and right

            char prefixDrawing = Ordinal == Cardinal ? UANR : VERR;
            IndentDrawing[Depth] = Ordinal == Cardinal ? NBSP : VERT;

            for (int i = 0; i < Depth; ++i)
                indent.Append(IndentDrawing.GetValueOrDefault(i, NBSP));

            char prefix = Depth == 0 ? NBSP : prefixDrawing;
            string line = $"{indent}{prefix}{Proc(BodyPart)}";
            indent.Clear();
            return line;
        }
        public static string GetLimbTree(Body Body,
            Func<BodyPart, string> Proc,
            int DepthLimit = int.MaxValue
            )
        {

        }
        */
    }
}
