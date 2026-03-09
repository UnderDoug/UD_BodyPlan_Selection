namespace UD_BodyPlan_Selection.Mod
{
    public static partial class Utils
    {
        public delegate T Parse<T>(string Value);

        public static Parse<T> GetVersionSafeParser<T>()
            => Startup.GetParser<T>()
            ?.ToParse()
            ;
    }
}
