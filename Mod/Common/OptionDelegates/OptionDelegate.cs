using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRL.Collections;
using XRL.World;

namespace UD_ChooseYourBodyPlan.Mod
{
    [Serializable]
    public class OptionDelegate
    {
        public static string[] ValidTags = new string[]
        {
            "Option",
            "OptionID",
            "Optional",
        };

        private static readonly Dictionary<string, Func<string, string, bool>> OperatorDelegates = new()
        {
            { "==" , EqualsNoCase },
            { "!=", NotEqualsNoCase },
            { ">", GreaterThan },
            { ">=", GreaterThanOrEqual },
            { "<", LessThan },
            { "<=", LessThanOrEqual },
        };

        public string OptionID;
        public string Operator;
        public string TrueWhen;

        protected Func<bool> Delegate;

        public OptionDelegate(string OptionID, string Operator, string TrueWhen)
        {
            SetFields(OptionID, Operator ?? "==", TrueWhen ?? "Yes");
        }
        public OptionDelegate((string OptionID, string Opertor, string TrueWhen) Parsed)
            : this(Parsed.OptionID, Parsed.Opertor, Parsed.TrueWhen)
        {
        }
        public OptionDelegate(string OptionPredicate)
            : this(ParseOptionPredicate(OptionPredicate))
        {
        }
        public OptionDelegate(OptionDelegate Source)
            : this(Source.OptionID, Source.Operator, Source.TrueWhen)
        {
        }

        public static (string OptionID, string Opertor, string TrueWhen) ParseOptionPredicate(string OptionPredicate)
        {
            (string OptionID, string Opertor, string TrueWhen) output = (null, null, null);
            int operatorCount = OperatorDelegates.Keys.Count(o => OptionPredicate.Contains(o));
            if (operatorCount > 1)
            {
                Utils.Error(new ArgumentException($"Must not contain more than one comparison operator.", nameof(OptionPredicate)));
            }
            else
            if (operatorCount == 1)
            {
                foreach ((var operatorString, var func) in OperatorDelegates)
                {
                    if (OptionPredicate.Contains(operatorString)
                        && OptionPredicate.Split(operatorString) is string[] operands)
                    {
                        output = (operands[0], operatorString, operands[1]);
                        break;
                    }
                }
            }
            else
            {
                output = (OptionPredicate, "==", "Yes");
            }
            return output;
        }
        public static bool TryParseOptionPredicate(string OptionPredicate, out string OptionID, out string Opertor, out string TrueWhen)
        {
            var parsed = ParseOptionPredicate(OptionPredicate);
            OptionID = parsed.OptionID;
            Opertor = parsed.Opertor;
            TrueWhen = parsed.TrueWhen;
            return OptionID.IsOption();
        }

        public bool IsValid()
            => (OptionID.IsNullOrEmpty()
                || OptionID.IsOption())
            && Delegate != null;

        public virtual OptionDelegate SetFields(string OptionID, string Operator = null, string TrueWhen = null)
        {
            if (OptionID.IsNullOrEmpty())
                Utils.Error(new ArgumentException($"Must not be null or empty.", nameof(OptionID)));
            else
            if (!OptionID.IsOption())
                Utils.Error(new ArgumentException($"Must be a valid option ID.", nameof(OptionID)));
            else
            {
                this.OptionID = OptionID;

                if (!Operator.IsNullOrEmpty())
                    this.Operator = Operator;

                if (!TrueWhen.IsNullOrEmpty())
                    this.TrueWhen = TrueWhen;

                Delegate = delegate ()
                {
                    return OperatorDelegates[this.Operator ?? "=="](XRL.UI.Options.GetOption(this.OptionID), this.TrueWhen);
                };
            }
            return this;
        }

        public virtual OptionDelegate ModifyTruth(string Operator, string TrueWhen)
            => SetFields(
                OptionID: OptionID,
                Operator: Operator ?? this.Operator,
                TrueWhen: TrueWhen ?? this.TrueWhen);

        public virtual OptionDelegate SetOperator(string Operator)
            => SetFields(
                OptionID: OptionID,
                Operator: Operator ?? this.Operator,
                TrueWhen: TrueWhen);

        public virtual OptionDelegate SetTrueWhen(string TrueWhen)
            => SetFields(
                OptionID: OptionID,
                Operator: Operator,
                TrueWhen: TrueWhen ?? this.TrueWhen);

        public virtual bool Check()
            => IsValid()
            && (Delegate?.Invoke() is not false
                || XRL.UI.Options.GetOption(OptionID) is not string option
                || option.EqualsNoCase(TrueWhen));

        public OptionDelegate Clone()
            => new(this);

        private static bool EqualsNoCase(string X, string Y)
            => X.EqualsNoCase(Y)
            ;

        private static bool NotEqualsNoCase(string X, string Y)
            => !Equals(X, Y)
            ;

        private static bool ParseOrError(string X, string Y, out int ResultX, out int ResultY)
        {
            ResultY = default;
            if (!int.TryParse(X.ToString(), out ResultX))
            {
                Utils.Error(new ArgumentException($"Cannot parse {X} to {typeof(int)}.", nameof(X)));
                return false;
            }

            if (!int.TryParse(Y.ToString(), out ResultY))
            {
                Utils.Error(new ArgumentException($"Cannot parse {Y} to {typeof(int)}.", nameof(Y)));
                return false;
            }
            return true;
        }

        private static bool GreaterThan(string X, string Y)
            => !ParseOrError(X, Y, out int resultX, out int resultY)
            || resultX > resultY
            ;

        private static bool GreaterThanOrEqual(string X, string Y)
            => !ParseOrError(X, Y, out int resultX, out int resultY)
            || resultX >= resultY
            ;

        private static bool LessThan(string X, string Y)
            => !ParseOrError(X, Y, out int resultX, out int resultY)
            || resultX < resultY
            ;

        private static bool LessThanOrEqual(string X, string Y)
            => !ParseOrError(X, Y, out int resultX, out int resultY)
            || resultX <= resultY
            ;
    }
}
