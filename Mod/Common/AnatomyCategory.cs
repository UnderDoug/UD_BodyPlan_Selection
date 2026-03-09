using System;
using System.Collections.Generic;
using System.Linq;

using XRL;
using XRL.Collections;
using XRL.World.Anatomy;

namespace UD_BodyPlan_Selection.Mod
{
    [HasModSensitiveStaticCache]
    public class AnatomyCategory
    {
        public class CategoryComparer : IComparer<AnatomyCategory>, IDisposable
        {
            public bool DefaultFirst;

            protected CategoryComparer()
            {
                DefaultFirst = false;
            }
            public CategoryComparer(bool DefaultFirst)
                : this()
            {
                this.DefaultFirst = DefaultFirst;
            }

            public int Compare(AnatomyCategory x, AnatomyCategory y)
            {
                if (y == null)
                {
                    if (x != null)
                        return -1;
                    else
                        return 0;
                }
                else
                if (x == null)
                    return 1;

                if (x.ID == 0)
                    return -1;
                if (y.ID == 0)
                    return 1;

                return string.Compare(x.DisplayName, y.DisplayName);
            }

            public void Dispose()
            {
            }
        }

        public static CategoryComparer Comparer = new(DefaultFirst: false);
        public static CategoryComparer DefaultFirstComparer = new(DefaultFirst: true);
        public static int LowestCategory => 1;
        public static int HighestCategory => 23;

        [ModSensitiveStaticCache]
        private static Dictionary<int, AnatomyCategory> _AnatomyCategories;
        public static Dictionary<int, AnatomyCategory> CategoryByID
        {
            get
            {
                if (_AnatomyCategories.IsNullOrEmpty())
                {
                    _AnatomyCategories ??= new();
                    for (int i = 0; i <= HighestCategory; i++)
                    {
                        try
                        {
                            _AnatomyCategories.Add(
                                key: i,
                                value: new() 
                                {
                                    ID = i,
                                    DisplayName = GetBodyPartCategoryName(i),
                                    Color = GetBodyPartCategoryColor(i),
                                    Choices = new()
                                });
                            Utils.Log(_AnatomyCategories[i]?.GetDisplayName());
                        }
                        catch (Exception x)
                        {
                            MetricsManager.LogModWarning(Utils.ThisMod, $"Attempted to make {nameof(AnatomyCategory)} from invalid {nameof(BodyPartCategory)} value: {i}; {x}");
                        }
                    }
                }
                return _AnatomyCategories;
            }
        }

        public static IEnumerable<AnatomyCategory> Categories => CategoryByID.Values;

        public static bool IsCodeValid(int Code)
            => Code == Math.Clamp(Code, LowestCategory, HighestCategory);

        public static string GetBodyPartCategoryName(int Code)
            => !IsCodeValid(Code)
            ? "Default"
            : BodyPartCategory.GetName(Code)
            ;

        public static string GetBodyPartCategoryColor(int Code)
        {
            if (IsCodeValid(Code)
                && BodyPartCategory.GetColor(Code) is string categoryColor)
                return categoryColor;

            if (GetBodyPartCategoryName(Code).ToLower() is string nameLower)
            {
                if (nameLower.ShaderColorOrNull() is string nameShader)
                    return nameShader;

                if ($"UD_BPS_{nameLower}".ShaderColorOrNull() is string nameCustomShader)
                    return nameCustomShader;
            }
            return null;
        }

        public static AnatomyCategory GetFor(AnatomyChoice Choice)
        {
            if (Choice == null)
                throw new ArgumentNullException(nameof(Choice));

            if (CategoryByID.IsNullOrEmpty())
                throw new InvalidOperationException($"{nameof(CategoryByID)} not initialized.");

            int categoryCode = Choice.Anatomy.BodyCategory
                ?? Choice.Anatomy.Category
                ?? Choice.Anatomy.Parts?.FirstOrDefault(p => p.Category != null)?.Category
                ?? 1;

            categoryCode = Math.Clamp(categoryCode, LowestCategory, HighestCategory);
            if (!CategoryByID.TryGetValue(categoryCode, out var category))
            {
                category = new()
                {
                    ID = categoryCode,
                    DisplayName = GetBodyPartCategoryName(categoryCode),
                    Color = GetBodyPartCategoryColor(categoryCode),
                    Choices = new(),
                };
            }
            category.RequireChoice(Choice);
            if (CategoryByID.TryGetValue(0, out var defaultCategory))
            {
                defaultCategory.RequireChoice(Choice);
                if (Choice.IsDefault)
                    category = defaultCategory;
            }

            return category;
        }

        public static bool TryGetFor(AnatomyChoice Choice, out AnatomyCategory Category)
        {
            Category = null;

            if (CategoryByID.IsNullOrEmpty())
            {
                MetricsManager.LogModWarning(Utils.ThisMod, $"{nameof(CategoryByID)} not initialized.");
                return false;
            }

            if (Choice?.Anatomy == null)
                return false;

            return (Category = GetFor(Choice)) != null;
        }

        public int ID;
        public string DisplayName;
        public string Color;

        public List<AnatomyChoice> Choices;

        public AnatomyCategory()
        {
            ID = -1;
            DisplayName = null;
            Color = null;
            Choices = new();
        }

        public string GetDisplayName()
            => !Color.IsNullOrEmpty() 
                && !DisplayName.IsNullOrEmpty()
            ? "{{" + $"{Color}|{DisplayName}" + "}}"
            : DisplayName;

        public bool IsValid(Predicate<AnatomyChoice> Filter = null)
            => !DisplayName.IsNullOrEmpty()
            && GetChoices() is IEnumerable<AnatomyChoice> choices
            && !choices.IsNullOrEmpty()
            && (Filter == null
                || choices.Any(Filter.Invoke))
            ;

        public bool IsDefaultMatching(AnatomyChoice Choice)
            => Choice != null
            && (ID == 0) == Choice.IsDefault;

        public IEnumerable<AnatomyChoice> GetChoices(Predicate<AnatomyChoice> Filter = null)
        {
            if (Choices.IsNullOrEmpty())
                yield break;

            Choices.StableSortInPlace((x, y) => string.Compare(x?.Anatomy?.Name, y?.Anatomy?.Name));

            foreach (var choice in Choices)
            {
                if (IsDefaultMatching(choice)
                    && choice.Anatomy != null
                    && (Filter == null
                        || Filter(choice)))
                {
                    yield return choice;
                }
            }
        }

        public void RequireChoice(AnatomyChoice Choice)
        {
            if (Choice != null
                && !Choices.Any(c => c?.Anatomy?.Name == Choice?.Anatomy?.Name))
                Choices.Add(Choice);
        }
    }
}
