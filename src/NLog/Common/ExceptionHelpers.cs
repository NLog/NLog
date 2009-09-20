using System;

namespace NLog.Common
{
    /// <summary>
    /// Helper functions for handling exceptions.
    /// </summary>
    public static class ExceptionHelpers
    {
        /// <summary>
        /// Function which returns the specified data ttype.
        /// </summary>
        public delegate T Func<T>();

        /// <summary>
        /// Tries to evaluate function, returns the default on exception.
        /// </summary>
        /// <typeparam name="T">Function return type</typeparam>
        /// <param name="function">The function.</param>
        /// <param name="errorMessage">The error message to be logged when the function throws.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static T ReturnDefaultOnException<T>(Func<T> function, string errorMessage, T defaultValue)
        {
            try
            {
                return function();
            }
            catch (Exception ex)
            {
                InternalLogger.Error(errorMessage, ex);
                return defaultValue;
            }
        }
    }
}
