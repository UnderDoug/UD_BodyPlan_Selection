using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ConsoleLib.Console;

using UnityEngine;

using XRL.Rules;
using XRL.UI;
using XRL.UI.Framework;

using ColorUtility = ConsoleLib.Console.ColorUtility;

namespace XRL.CharacterBuilds.Qud.UI
{
    [UIView(
        ID: "CharacterCreation:UD_PickBodyPlan",
        NavCategory: "Chargen",
        UICanvas: "Chargen/PickCybernetics",
        UICanvasHost: 1)]
    public class Qud_UD_BodyPlanModuleWindow : EmbarkBuilderModuleWindowPrefabBase<Qud_UD_BodyPlanModule, CategoryMenusScroller>
    {
        protected const string EMPTY_CHECK = "[ ]";

        protected const string CHECKED = "[■]";

        private List<CategoryMenuData> AnatomiesMenuState = new();

        private List<Qud_UD_BodyPlanModule.AnatomyChoice> AnatomyChoices => module?.AnatomyChoices;

        public override void BeforeShow(EmbarkBuilderModuleWindowDescriptor descriptor)
        {
            if (module.data == null)
            {
                module.setData(module.GetDefaultData());

                module.OrganizeAnatomyChoices(true);
            }
            
            prefabComponent.onSelected.RemoveAllListeners();
            prefabComponent.onSelected.AddListener(SelectAnatomy);

            UpdateControls();

            base.BeforeShow(descriptor);
        }

        public override GameObject InstantiatePrefab(GameObject prefab)
        {
            prefab.GetComponentInChildren<CategoryMenusScroller>().allowVerticalLayout = false;
            return base.InstantiatePrefab(prefab);
        }

        public override void RandomSelectionNoUI()
        {
            module.PickAnatomy(Stat.Roll(0, module.AnatomyChoices.Count - 1));
            UpdateControls();
        }

        public override void RandomSelection()
        {
            int num = Stat.Roll(0, module.AnatomyChoices.Count - 1);
            prefabComponent.ContextFor(0, num).ActivateAndEnable();
            module.PickAnatomy(num);
            UpdateControls();
        }

        public override void ResetSelection()
        {
            module.PickAnatomy(0);
            UpdateControls();
        }

        public override UIBreadcrumb GetBreadcrumb()
        {
            Renderable renderable = (module?.SelectedChoice())?.GetRenderable();
            return new()
            {
                Id = GetType().FullName,
                Title = (module?.SelectedChoice())?.GetDescription() ?? "Body Plan",
                IconPath = renderable?.getTile() ?? "Creatures/natural-weapon-fist.bmp",
                IconDetailColor = ColorUtility.ColorMap[renderable?.getColorChars().detail ?? 'W'],
                IconForegroundColor = ColorUtility.ColorMap[renderable?.getColorChars().foreground ?? 'w']
            };
        }

        public void SelectAnatomy(FrameworkDataElement dataElement)
        {
            module.PickAnatomy(AnatomiesMenuState[0].menuOptions.FindIndex(d => d == dataElement));
            UpdateControls();
        }

        public void UpdateControls()
        {
            AnatomiesMenuState = new();
            var categoryMenuData = new CategoryMenuData
            {
                Title = "Body Plans",
                menuOptions = new()
            };
            AnatomiesMenuState.Add(categoryMenuData);
            if (AnatomyChoices != null)
                foreach (Qud_UD_BodyPlanModule.AnatomyChoice choice in AnatomyChoices)
                {
                    if (choice == null)
                        continue;

                    bool isSelected = module.IsSelected(choice);
                    string description = choice.GetDescription();
                    if (isSelected)
                        description = "{{W|" + description + "}}";

                    categoryMenuData.menuOptions.Add(
                        item: new PrefixMenuOption()
                        {
                            Prefix = isSelected ? CHECKED : EMPTY_CHECK,
                            Description = description,
                            LongDescription = choice.GetLongDescription(IncludeOpening: true),
                            Renderable = choice.GetRenderable()
                        });
                }

            if (!module.builder.SkippingUIUpdates)
                prefabComponent.BeforeShow(descriptor, AnatomiesMenuState);
        }
    }
}
