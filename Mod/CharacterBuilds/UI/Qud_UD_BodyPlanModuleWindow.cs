using System;
using System.Collections.Generic;
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
        public EmbarkBuilderModuleWindowDescriptor windowDescriptor;

        protected const string EMPTY_CHECK = "[ ]";

        protected const string CHECKED = "[■]";

        private List<CategoryMenuData> AnatomiesMenuState = new();

        public void PickAnatomy(int n)
        {
            module.setData(new Qud_UD_BodyPlanModuleData(module.AnatomyChoices[n].Anatomy));
            UpdateControls();
        }

        public override void BeforeShow(EmbarkBuilderModuleWindowDescriptor descriptor)
        {
            if (descriptor != null)
                windowDescriptor = descriptor;

            if (module.data == null)
                module.setData(module.GetDefaultData());

            prefabComponent.onSelected.RemoveAllListeners();
            prefabComponent.onSelected.AddListener(SelectAnatomy);

            UpdateControls();

            base.BeforeShow(descriptor);
        }

        public void SelectAnatomy(FrameworkDataElement dataElement)
            => PickAnatomy(AnatomiesMenuState[0].menuOptions.FindIndex(d => d == dataElement));

        public void UpdateControls()
        {
            AnatomiesMenuState = new();
            var categoryMenuData = new CategoryMenuData
            {
                Title = "Body Plans",
                menuOptions = new()
            };
            AnatomiesMenuState.Add(categoryMenuData);
            for (int i = 0; i < module.AnatomyChoices.Count; i++)
            {
                var item = new PrefixMenuOption()
                {
                    Prefix = module.IsSelected(module.AnatomyChoices[i]) ? "[■]" : "[ ]",
                    Description = module.AnatomyChoices[i].GetDescription(),
                    LongDescription = module.AnatomyChoices[i].GetLongDescription(),
                    Renderable = module.AnatomyChoices[i].GetRenderable()
                };
                categoryMenuData.menuOptions.Add(item);
            }

            if (!module.builder.SkippingUIUpdates)
                prefabComponent.BeforeShow(windowDescriptor, AnatomiesMenuState);

        }

        public override void ResetSelection()
        {
            module.setData(module.GetDefaultData());
            UpdateControls();
        }

        public override void RandomSelectionNoUI()
            => PickAnatomy(Stat.Roll(0, module.AnatomyChoices.Count - 1));

        public override void RandomSelection()
        {
            int num = Stat.Roll(0, module.AnatomyChoices.Count - 1);
            prefabComponent.ContextFor(0, num).ActivateAndEnable();
            PickAnatomy(num);
            UpdateControls();
        }

        public override GameObject InstantiatePrefab(GameObject prefab)
        {
            prefab.GetComponentInChildren<CategoryMenusScroller>().allowVerticalLayout = false;
            return base.InstantiatePrefab(prefab);
        }

        public override UIBreadcrumb GetBreadcrumb()
        {
            Renderable renderable = (module?.SelectedChoice())?.GetRenderable();
            return new()
            {
                Id = GetType().FullName,
                Title = (module?.SelectedChoice())?.Anatomy?.Name ?? "Body Plan",
                IconPath = renderable?.getTile() ?? "creatures/sw_slog.bmp",
                IconDetailColor = ColorUtility.ColorMap[renderable?.getColorChars().detail ?? 'W'],
                IconForegroundColor = ColorUtility.ColorMap[renderable?.getColorChars().foreground ?? 'w']
            };
        }
    }
}
