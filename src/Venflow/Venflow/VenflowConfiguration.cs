using System.Diagnostics;
using System.Reflection;

namespace Venflow
{
    /// <summary>
    /// Contains methods to globally set the configuration of Venflow.
    /// </summary>
    public static class VenflowConfiguration
    {
        /// <summary>
        /// Determines whether or not Venflow will perform more extensive validation through out its usage. This setting will be set to <see langword="true"></see> automatically, if you are in DEBUG, otherwise <see langword="false"></see>.
        /// </summary>
        public static bool ShouldUseDeepValidation { get; private set; }

        internal static bool PopulateColumnInformation { get; set; }

        private static bool _validationSettingSet;

        /// <summary>
        /// Changes the value of the <see cref="ShouldUseDeepValidation"/> property.
        /// </summary>
        /// <param name="validation">Determines if Venflow should use deep validation or not.</param>
        public static void UseDeepValidation(bool validation)
        {
            ShouldUseDeepValidation = validation;
            _validationSettingSet = true;
        }

        internal static void SetDefaultValidationIfNeeded(Assembly assembly)
        {
            if (_validationSettingSet)
                return;

            var attribute = assembly.GetCustomAttribute<DebuggableAttribute>();

            if (attribute is null)
            {
                UseDeepValidation(false);

                return;
            }

            UseDeepValidation(attribute.IsJITTrackingEnabled);
        }
    }
}
