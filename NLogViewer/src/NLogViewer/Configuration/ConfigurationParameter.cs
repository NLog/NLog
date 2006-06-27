// 
// Copyright (c) 2004,2005 Jaroslaw Kowalski <jkowalski@users.sourceforge.net>
// 
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
// * Neither the name of the Jaroslaw Kowalski nor the names of its 
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
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Reflection;
using System.Globalization;

namespace NLogViewer.Configuration
{
    /// <summary>
    /// Represents a parameter to the receiver or parser
    /// </summary>
    /// <remarks>
    /// The parameters are uninterpreted (name,value) string pairs.
    /// </remarks>
	public class ConfigurationParameter
	{
        /// <summary>
        /// Creates a new instance of <see cref="ConfigurationParameter"/>.
        /// </summary>
        public ConfigurationParameter()
        {
        }
        
        /// <summary>
        /// Creates a new instance of <see cref="ConfigurationParameter"/> and sets the parameter name and value.
        /// </summary>
        public ConfigurationParameter(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }

        /// <summary>
        /// ConfigurationParameter name.
        /// </summary>
        [XmlAttribute("name")]
        public string Name;

        /// <summary>
        /// ConfigurationParameter value.
        /// </summary>
        [XmlAttribute("value")]
        public string Value;

        public static void ApplyConfigurationParameters(object target, List<ConfigurationParameter> parameters)
        {
            Type targetType = target.GetType();

            foreach (ConfigurationParameter cp in parameters)
            {
                PropertyInfo pi = targetType.GetProperty(cp.Name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance);
                if (pi != null)
                {
                    object typedValue = Convert.ChangeType(cp.Value, pi.PropertyType, CultureInfo.InvariantCulture);
                    pi.SetValue(target, typedValue, null);
                }
            }
        }

        public static List<ConfigurationParameter> CaptureConfigurationParameters(object target)
        {
            List<ConfigurationParameter> result = new List<ConfigurationParameter>();
            Type targetType = target.GetType();

            foreach (PropertyInfo pi in targetType.GetProperties(BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance))
            {
                if (pi.IsDefined(typeof(XmlIgnoreAttribute), true))
                    continue;

                object v = pi.GetValue(target, null);
                if (v != null)
                {
                    string stringValue = Convert.ToString(v, CultureInfo.InvariantCulture);
                    result.Add(new ConfigurationParameter(pi.Name, stringValue));
                }
            }
            return result;
        }
    }
}
