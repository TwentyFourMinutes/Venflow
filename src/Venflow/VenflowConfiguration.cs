using System;
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
        /// Determines the expiration duration time of the underlying SQL Materializer Cache in seconds, defaults to 5 minutes.
        /// </summary>
        /// <remarks>
        /// Venflow uses the SQL used in queries to map a materializer to this specific query. This is faster, than using the returned columns and used relations as a cache key. If a query with the executed SQL won't be called again within the next <see cref="DynamicCacheExpirationTime"/>, it will be removed from the cache. However do note, that the materializer itself will not be removed from the cache.
        /// </remarks>
        public static long DynamicCacheExpirationTime { get; private set; } = 60 * 5;

        /// <summary>
        /// Determines whether or not Venflow will propagate exceptions to the caller of a command, if the exception is being logged. Defaults to <see langword="true"/>.
        /// </summary>
        public static bool ThrowLoggedExceptions { get; set; } = true;

        /// <summary>
        /// Determines whether or not Venflow will perform more extensive validation through out its usage. This setting will be set to <see langword="true"></see> automatically, if you are in DEBUG, otherwise <see langword="false"></see>.
        /// </summary>
        public static bool ShouldUseDeepValidation { get; private set; }

        private static bool _validationSettingSet;

        /// <summary>
        /// Sets the value of the <see cref="DynamicCacheExpirationTime"/> property.
        /// </summary>
        /// <param name="timeSpan">The expiration duration time.</param>
        public static void SetDynamicCacheExpirationTime(TimeSpan timeSpan)
        {
            var seconds = (long)timeSpan.TotalSeconds;

            if (seconds <= 0)
                throw new ArgumentException("The timeSpan needs to be larger than 0 seconds.", nameof(timeSpan));

            DynamicCacheExpirationTime = seconds;
        }

        /// <summary>
        /// Sets the value of the <see cref="DynamicCacheExpirationTime"/> property.
        /// </summary>
        /// <param name="expirationTime">The expiration duration time in seconds.</param>
        public static void SetDynamicCacheExpirationTime(long expirationTime)
        {
            if (expirationTime <= 0)
                throw new ArgumentException("The expirationTime needs to be larger than 0 seconds.", nameof(expirationTime));

            DynamicCacheExpirationTime = expirationTime;
        }

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
