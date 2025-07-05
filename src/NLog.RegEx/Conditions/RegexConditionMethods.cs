using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace NLog.Conditions
{
    /// <summary>
    /// A bunch of utility methods (mostly predicates) which can be used in
    /// condition expressions. Partially inspired by XPath 1.0.
    /// </summary>
    [ConditionMethods]
    public static class RegexConditionMethods
    {
        /// <summary>
        /// Indicates whether the specified regular expression finds a match in the specified input string.
        /// </summary>
        /// <param name="input">The string to search for a match.</param>
        /// <param name="pattern">The regular expression pattern to match.</param>
        /// <param name="options">A string consisting of the desired options for the test. The possible values are those of the <see cref="RegexOptions"/> separated by commas.</param>
        /// <returns><see langword="true"/> if the regular expression finds a match; otherwise, <see langword="false"/>.</returns>
        [ConditionMethod("regex-matches")]
        public static bool RegexMatches(string input, string pattern, [Optional, DefaultParameterValue("")] string options)
        {
            RegexOptions regexOpts = ParseRegexOptions(options) | RegexOptions.ExplicitCapture;
            return Regex.IsMatch(input, pattern, regexOpts);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        private static RegexOptions ParseRegexOptions(string options)
        {
            if (string.IsNullOrEmpty(options))
            {
                return RegexOptions.None;
            }

            return (RegexOptions)Enum.Parse(typeof(RegexOptions), options, true);
        }
    }
}
