namespace Venflow
{
    public static class VenflowConfiguration
    {
        internal static bool ValidationSettingSet { get; private set; }

        public static bool ShouldUseDeepValidation { get; private set; }

        public static void UseDeepValidation(bool validation)
        {
            ShouldUseDeepValidation = validation;
            ValidationSettingSet = true;
        }
    }
}
