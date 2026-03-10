using System;
using System.Collections.Generic;
using System.Text;

namespace UD_BodyPlan_Selection.Mod
{
    public class TextAddition
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
        public List<string> Description;
        public List<string> Summary;
        public List<Symbol> Symbols;
    }
}
