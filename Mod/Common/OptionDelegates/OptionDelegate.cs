using System;
using System.Collections.Generic;
using System.Text;

namespace UD_ChooseYourBodyPlan.Mod
{
    public class OptionDelegate : BaseOptionDelegate
    {
        public override bool EnforceValidOptionID => false;

        public OptionDelegate()
            : base()
        { }

        public OptionDelegate(string OptionID)
            : base(OptionID)
        { }

        public OptionDelegate(string OptionID, string TrueState)
            : base(OptionID, TrueState)
        { }

        public OptionDelegate(string OptionID, string Operator, string TrueState)
            : base(OptionID, Operator, TrueState)
        { }
    }
}
