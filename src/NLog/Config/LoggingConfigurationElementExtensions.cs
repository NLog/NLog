// 
// Copyright (c) 2004-2020 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
// 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without 
// modification, are permitted provided that the following conditions 
// are met:
// 
// * Redistributions of source code must retain the above copyright notice, 
//   this list of conditions and the following disclaimer. 
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution. 
// 
// * Neither the name of Jaroslaw Kowalski nor the names of its 
//   contributors may be used to endorse or promote products derived from this
//   software without specific prior written permission. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
// THE POSSIBILITY OF SUCH DAMAGE.
// 

using System;
using System.Globalization;
using System.Linq;
using NLog.Common;
using NLog.Internal;

namespace NLog.Config
{
    internal static class LoggingConfigurationElementExtensions
    {
        public static bool MatchesName(this ILoggingConfigurationElement section, string expectedName)
        {
            return string.Equals(section?.Name?.Trim(), expectedName, StringComparison.OrdinalIgnoreCase);
        }

        public static void AssertName(this ILoggingConfigurationElement section, params string[] allowedNames)
        {
            foreach (var en in allowedNames)
            {
                if (section.MatchesName(en))
                    return;
            }

            throw new InvalidOperationException(
                $"Assertion failed. Expected element name '{string.Join("|", allowedNames)}', actual: '{section?.Name}'.");
        }

        public static string GetRequiredValue(this ILoggingConfigurationElement element, string attributeName, string section)
        {
            string value = element.GetOptionalValue(attributeName, null);
            if (value == null)
            {
                throw new NLogConfigurationException($"Expected {attributeName} on {element.Name} in {section}");
            }

            if (StringHelpers.IsNullOrWhiteSpace(value))
            {
                throw new NLogConfigurationException(
                    $"Expected non-empty {attributeName} on {element.Name} in {section}");
            }

            return value;
        }

        public static string GetOptionalValue(this ILoggingConfigurationElement element, string attributeName, string defaultValue)
        {
            return element.Values
                .Where(configItem => string.Equals(configItem.Key, attributeName, StringComparison.OrdinalIgnoreCase))
                .Select(configItem => configItem.Value).FirstOrDefault() ?? defaultValue;
        }

        /// <summary>
        /// Gets the optional boolean attribute value.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="defaultValue">Default value to return if the attribute is not found or if there is a parse error</param>
        /// <returns>Boolean attribute value or default.</returns>
        public static bool GetOptionalBooleanValue(this ILoggingConfigurationElement element, string attributeName,
            bool defaultValue)
        {
            string value = element.GetOptionalValue(attributeName, null);
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }

            try
            {
                return Convert.ToBoolean(value.Trim(), CultureInfo.InvariantCulture);
            }
            catch (Exception exception)
            {
                var configException = new NLogConfigurationException(exception, $"'{attributeName}' hasn't a valid boolean value '{value}'. {defaultValue} will be used");
                if (configException.MustBeRethrown())
                {
                    throw configException;
                }
                InternalLogger.Error(exception, configException.Message);
                return defaultValue;
            }
        }

        public static string GetConfigItemTypeAttribute(this ILoggingConfigurationElement element, string sectionNameForRequiredValue = null)
        {
            var typeAttributeValue = sectionNameForRequiredValue != null ? element.GetRequiredValue("type", sectionNameForRequiredValue) : element.GetOptionalValue("type", null);
            return StripOptionalNamespacePrefix(typeAttributeValue)?.Trim();
        }

        /// <summary>
        /// Remove the namespace (before :)
        /// </summary>
        /// <example>
        /// x:a, will be a
        /// </example>
        /// <param name="attributeValue"></param>
        /// <returns></returns>
        private static string StripOptionalNamespacePrefix(string attributeValue)
        {
            if (attributeValue == null)
            {
                return null;
            }

            int p = attributeValue.IndexOf(':');
            if (p < 0)
            {
                return attributeValue;
            }

            return attributeValue.Substring(p + 1);
        }
    }
}
