using System;
using System.Collections.Generic;
using System.Text;

using XRL;
using XRL.Collections;

namespace UD_ChooseYourBodyPlan.Mod
{
    public class TextElements : ILoadFromDataBucket<TextElements>
    {
        public struct Symbol
        {
            public string Name;
            public char Color;
            public char Value;

            public Symbol(string Name, char Color, char Value)
            {
                this.Name = Name;
                this.Color = Color;
                this.Value = Value;
            }

            public override readonly string ToString()
                => Color != '\0'
                ? "{{" + $"{Color}|{Value}" + "}}"
                : Value.ToString()
                ;
        }

        public string Name;
        public List<string> DescriptionBefore;
        public List<string> DescriptionAfter;
        public List<string> SummaryBefore;
        public List<string> SummaryAfter;

        public StringMap<Symbol> SymbolsByName;

        private List<Symbol> _Symbols;
        public List<Symbol> Symbols
        {
            get
            {
                if (_Symbols.IsNullOrEmpty())
                {
                    _Symbols = new();
                    using var values = SymbolsByName.Values.GetEnumerator();
                    while (values.MoveNext())
                        _Symbols.Add(values.Current);
                }
                return _Symbols;
            }
        }
    }
}
