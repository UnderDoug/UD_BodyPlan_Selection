using System;
using System.Collections.Generic;
using System.Linq;

using XRL;
using XRL.World;
using XRL.World.Anatomy;

using static UD_ChooseYourBodyPlan.Mod.AnatomyConfiguration;

namespace UD_ChooseYourBodyPlan.Mod
{
    public class TransformationData : ILoadFromDataBucket<TransformationData>
    {
        public static string RemoveTag => Const.REMOVE_TAG;

        public BodyPlanRenderable Render;
        public string RenderString;
        public string Tile;
        public string TileColor;
        public string DetailColor;
        public string Species;
        public string Property;
        public List<string> Mutations;

        public TransformationData()
        {
            RenderString = null;
            Tile = null;
            Tile = null;
            DetailColor = null;
            Species = null;
            Property = null;
            Mutations = null;
        }
        public TransformationData(Dictionary<string, string> xTag)
            : this()
        {
            if (xTag != null)
            {
                Render = new(xTag);
                xTag.AssignStringFieldFromXTag(nameof(RenderString), ref RenderString);
                xTag.AssignStringFieldFromXTag(nameof(Tile), ref Tile);
                xTag.AssignStringFieldFromXTag(nameof(TileColor), ref TileColor);
                xTag.AssignStringFieldFromXTag(nameof(DetailColor), ref DetailColor);
                xTag.AssignStringFieldFromXTag(nameof(Species), ref Species);
                xTag.AssignStringFieldFromXTag(nameof(Property), ref Property);

                if (xTag.TryGetValue(nameof(Mutations), out string mutations)
                    && !mutations.EqualsNoCase(RemoveTag))
                    Mutations = Utils.GetVersionSafeParser<List<string>>()?.Invoke(mutations);
            }
        }
        public TransformationData(GameObjectBlueprint DataBucket)
            : this()
        {
            if (DataBucket.InheritsFrom(Const.XFORM_DATA_BLUEPRINT))
            {
                DataBucket.TryGetTagValueForData(nameof(Species), out Species);
                DataBucket.TryGetTagValueForData(nameof(Property), out Property);

                Render = new(DataBucket);
                if (!DataBucket.Mutations.IsNullOrEmpty())
                    Mutations = new(DataBucket.Mutations.Keys);

                if (DataBucket.TryGetTag(nameof(Mutations), out string mutations)
                    && mutations.CachedCommaExpansion().ToList() is List<string> mutationsList
                    && !mutationsList.IsNullOrEmpty())
                {
                    foreach (var mutation in mutationsList)
                    {
                        if (MutationFactory.GetMutationEntryByName(mutation) is not MutationEntry mutationEntry)
                            continue;

                        if (!Mutations.Select(m => MutationFactory.GetMutationEntryByName(m)).Contains(mutationEntry))
                            Mutations.Add(mutation);
                    }
                }
            }
            else
            {
                Utils.ThisMod.Error($"Aborted attempt to construct {GetType().Name} " +
                    $"from DataBucket inheriting from \"{DataBucket.GetBase()}\" " +
                    $"instead of \"{Const.XFORM_DATA_BLUEPRINT}\"");
            }
        }

        public void DebugOutput(int Indent = 0)
        {
            Utils.Log($"{nameof(RenderString)}: {RenderString ?? "NO_RENDER_STRING"}", Indent: Indent);
            Utils.Log($"{nameof(Tile)}: {Tile ?? "NO_TILE"}", Indent: Indent);
            Utils.Log($"{nameof(TileColor)}: {TileColor ?? "NO_TILE_COLOR"}", Indent: Indent);
            Utils.Log($"{nameof(DetailColor)}: {DetailColor ?? "NO_DETAIL_COLOR"}", Indent: Indent);
            Utils.Log($"{nameof(Species)}: {Species ?? "NO_SPECIES"}", Indent: Indent);
            Utils.Log($"{nameof(Property)}: {Property ?? "NO_PROPERTY"}", Indent: Indent);
            Utils.Log($"{nameof(Mutations)}:", Indent: Indent);
            if (Mutations.IsNullOrEmpty())
                Utils.Log("::None", Indent: Indent + 1);
            else
                foreach (string mutation in Mutations)
                    Utils.Log($"::{mutation}", Indent: Indent + 1);
        }
    }
}
