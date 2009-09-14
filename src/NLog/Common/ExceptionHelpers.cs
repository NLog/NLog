using System;

namespace NLog.Common
{
    public static class ExceptionHelpers
    {
        public delegate void Action();

        public delegate T Func<T>();

        public static void IgnoreExceptions(Action action, string errorMessage)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                InternalLogger.Error(errorMessage, ex);
                throw;
            }
        }

        public static T ReturnDefaultOnException<T>(Func<T> action, string errorMessage, T defaultValue)
        {
            try
            {
                return action();
            }
            catch (Exception ex)
            {
                InternalLogger.Error(errorMessage, ex);
                return defaultValue;
            }
        }
    }
}
