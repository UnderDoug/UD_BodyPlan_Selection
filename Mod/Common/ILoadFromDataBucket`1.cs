using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;

namespace UD_ChooseYourBodyPlan.Mod
{
    public interface ILoadFromDataBucket<T>
        where T : new()
    {
        T LoadFromDataBucket(GameObjectBlueprint DataBucket);
    }
}
