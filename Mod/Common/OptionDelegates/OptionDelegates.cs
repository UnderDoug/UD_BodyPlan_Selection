using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRL.Collections;
using XRL.World;

namespace UD_ChooseYourBodyPlan.Mod
{
    [Serializable]
    public class OptionDelegates : List<OptionDelegate>
    {
        public OptionDelegates()
        {
        }

        public OptionDelegates(IReadOnlyList<OptionDelegate> Source)
            : this()
        {
            if (!Source.IsNullOrEmpty())
                foreach (var optionDelegate in Source)
                    this.Merge(optionDelegate.Clone());
        }

        public OptionDelegates(OptionDelegates Source)
            : this((IReadOnlyList<OptionDelegate>)Source)
        {
        }

        public virtual bool Check()
            => this.CheckAll()
            ;

        public OptionDelegates Merge(OptionDelegates Other)
        {
            if (!Other.IsNullOrEmpty())
            {
                foreach (var optionDelegate in Other)
                    this.Merge(optionDelegate);
            }
            return this;
        }

        public OptionDelegates Clone()
            => new(this);
    }
}
