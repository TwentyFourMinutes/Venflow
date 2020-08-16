namespace Venflow
{
    /// <summary>
    /// Contains methods to globally set the configuration of Venflow.
    /// </summary>
    public static class VenflowConfiguration
    {
        internal static bool ValidationSettingSet { get; private set; }

        /// <summary>
        /// Determines whether or not Venflow will perform more extensive validation through out its usage. This setting will be set to <see langword="true"></see> automatically, if you are in DEBUG, otherwise <see langword="false"></see>.
        /// </summary>
        public static bool ShouldUseDeepValidation { get; private set; }

        /// <summary>
        /// Changes the value of the <see cref="ShouldUseDeepValidation"/> property.
        /// </summary>
        /// <param name="validation">Determines if Venflow should use deep validation or not.</param>
        public static void UseDeepValidation(bool validation)
        {
            ShouldUseDeepValidation = validation;
            ValidationSettingSet = true;
        }
    }
}
