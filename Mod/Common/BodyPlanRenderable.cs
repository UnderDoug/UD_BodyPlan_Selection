using System.Collections.Generic;

using ConsoleLib.Console;

using XRL;
using XRL.World;
using XRL.World.Anatomy;
using static UD_ChooseYourBodyPlan.Mod.AnatomyConfiguration;

namespace UD_ChooseYourBodyPlan.Mod
{
    [HasModSensitiveStaticCache]
    public class BodyPlanRenderable : Renderable, ILoadFromDataBucket<BodyPlanRenderable>
    {
        public static string RemoveTag => "*remove";
        public static string xTagPrefix => Const.MOD_PREFIX_SHORT;

        [ModSensitiveStaticCache]
        private static Dictionary<string, BodyPlanRenderable> _BodyPlanRenderables;
        public static Dictionary<string, BodyPlanRenderable> BodyPlanRenderables
        {
            get
            {
                if (_BodyPlanRenderables.IsNullOrEmpty())
                {
                    _BodyPlanRenderables = new();
                    Utils.Log($"Caching {nameof(BodyPlanRenderables)}:");
                    foreach (var blueprint in GameObjectFactory.Factory?.GetBlueprintsInheritingFrom(Const.BODYPLAN_ENTRY_BLUEPRINT))
                    {
                        if (blueprint.TryGetTag(nameof(Anatomy), out string anatomy))
                            _BodyPlanRenderables[anatomy] = new(blueprint);

                        Utils.Log(
                            $"{blueprint.Name}, " +
                            $"{nameof(Anatomy)}: {blueprint.GetTag(nameof(Anatomy), "NO_ANATOMY_TAG")}, " +
                            $"Tile: {_BodyPlanRenderables.GetValue(anatomy)?.Tile}", Indent: 1);
                    }
                }
                return _BodyPlanRenderables;
            }
        }

        public bool HFlip;

        public BodyPlanRenderable(
            string Tile,
            string RenderString = null,
            string ColorString = null,
            string TileColor = null,
            char DetailColor = '\0',
            bool HFlip = false)
            : base(
                  Tile: Tile,
                  RenderString: RenderString,
                  ColorString: ColorString,
                  TileColor: TileColor,
                  DetailColor: DetailColor)
        {
            this.HFlip = HFlip;
        }
        public BodyPlanRenderable(TransformationData Transformation, bool HFlip = false)
            : this(
                  Tile: Transformation?.Tile,
                  RenderString: Transformation?.RenderString ?? "@",
                  ColorString: $"{Transformation?.TileColor ?? "&Y"}^{Transformation?.DetailColor ?? "y"}",
                  TileColor: Transformation?.TileColor ?? "&Y",
                  DetailColor: Transformation?.DetailColor?[0] ?? 'y',
                  HFlip: HFlip)
        { }
        public BodyPlanRenderable(GenotypeEntry GenotypeEntry)
            : this(
                  Tile: GenotypeEntry.Tile,
                  RenderString: "@",
                  ColorString: $"&Y^{GenotypeEntry.DetailColor}",
                  TileColor: "&Y",
                  DetailColor: GenotypeEntry?.DetailColor?[0] ?? 'y',
                  HFlip: true)
        { }
        public BodyPlanRenderable(SubtypeEntry SubtypeEntry)
            : this(
                  Tile: SubtypeEntry.Tile,
                  RenderString: "@",
                  ColorString: $"&Y^{SubtypeEntry.DetailColor}",
                  TileColor: "&Y",
                  DetailColor: SubtypeEntry?.DetailColor?[0] ?? 'y',
                  HFlip: true)
        { }
        public BodyPlanRenderable(Renderable Renderable, bool HFlip = false)
            : base(Renderable)
        {
            this.HFlip = HFlip;
        }
        public BodyPlanRenderable(GameObjectBlueprint Blueprint, bool HFlip = false)
            : base(Blueprint)
        {
            this.HFlip = HFlip;
        }
        public BodyPlanRenderable(Dictionary<string, string> xTag, bool HFlip = false)
            : base()
        {
            this.HFlip = HFlip;

            if (!xTag.IsNullOrEmpty())
            {
                xTag.AssignStringFieldFromXTag(nameof(Tile), ref Tile);

                xTag.AssignStringFieldFromXTag(nameof(RenderString), ref RenderString);

                xTag.AssignStringFieldFromXTag(nameof(ColorString), ref ColorString);

                xTag.AssignStringFieldFromXTag(nameof(TileColor), ref TileColor);

                if (xTag.TryGetValue(nameof(DetailColor), out string detailColor)
                    && !detailColor.EqualsNoCase(RemoveTag))
                    DetailColor = detailColor?[0] ?? '\0';

                if (xTag.TryGetValue(nameof(this.HFlip), out string hFlip))
                    bool.TryParse(hFlip, out this.HFlip);
            }
        }
        public BodyPlanRenderable(string Anatomy, bool HFlip = false)
            : this(
                  Renderable: BodyPlanRenderables?.ContainsKey(Anatomy) ?? false
                    ? BodyPlanRenderables[Anatomy]
                    : null,
                  HFlip: HFlip)
        { }

        public override bool getHFlip()
            => HFlip;
    }
}
