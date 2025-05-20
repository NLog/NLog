//
// Copyright (c) 2004-2024 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

#if !NET5_0_OR_GREATER

namespace System.Diagnostics.CodeAnalysis
{
    [AttributeUsage(
        AttributeTargets.Field | AttributeTargets.ReturnValue | AttributeTargets.GenericParameter |
        AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.Method |
        AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct,
        Inherited = false)]
    internal sealed class DynamicallyAccessedMembersAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicallyAccessedMembersAttribute"/> class
        /// with the specified member types.
        /// </summary>
        /// <param name="memberTypes">The types of members dynamically accessed.</param>
        public DynamicallyAccessedMembersAttribute(DynamicallyAccessedMemberTypes memberTypes)
        {
            MemberTypes = memberTypes;
        }

        /// <summary>
        /// Gets the <see cref="DynamicallyAccessedMemberTypes"/> which specifies the type
        /// of members dynamically accessed.
        /// </summary>
        public DynamicallyAccessedMemberTypes MemberTypes { get; }
    }

    /// <summary>
    /// Specifies the types of members that are dynamically accessed.
    ///
    /// This enumeration has a <see cref="FlagsAttribute"/> attribute that allows a
    /// bitwise combination of its member values.
    /// </summary>
    [Flags]
    internal enum DynamicallyAccessedMemberTypes
    {
        /// <summary>
        /// Specifies no members.
        /// </summary>
        None = 0,

        /// <summary>
        /// Specifies the default, parameterless public constructor.
        /// </summary>
        PublicParameterlessConstructor = 0x0001,

        /// <summary>
        /// Specifies all public constructors.
        /// </summary>
        PublicConstructors = 0x0002 | PublicParameterlessConstructor,

        /// <summary>
        /// Specifies all non-public constructors.
        /// </summary>
        NonPublicConstructors = 0x0004,

        /// <summary>
        /// Specifies all public methods.
        /// </summary>
        PublicMethods = 0x0008,

        /// <summary>
        /// Specifies all non-public methods.
        /// </summary>
        NonPublicMethods = 0x0010,

        /// <summary>
        /// Specifies all public fields.
        /// </summary>
        PublicFields = 0x0020,

        /// <summary>
        /// Specifies all non-public fields.
        /// </summary>
        NonPublicFields = 0x0040,

        /// <summary>
        /// Specifies all public nested types.
        /// </summary>
        PublicNestedTypes = 0x0080,

        /// <summary>
        /// Specifies all non-public nested types.
        /// </summary>
        NonPublicNestedTypes = 0x0100,

        /// <summary>
        /// Specifies all public properties.
        /// </summary>
        PublicProperties = 0x0200,

        /// <summary>
        /// Specifies all non-public properties.
        /// </summary>
        NonPublicProperties = 0x0400,

        /// <summary>
        /// Specifies all public events.
        /// </summary>
        PublicEvents = 0x0800,

        /// <summary>
        /// Specifies all non-public events.
        /// </summary>
        NonPublicEvents = 0x1000,

        /// <summary>
        /// Specifies all interfaces implemented by the type.
        /// </summary>
        Interfaces = 0x2000,

        /// <summary>
        /// Specifies all members.
        /// </summary>
        All = ~None
    }

    /// <summary>
    /// Suppresses reporting of a specific rule violation, allowing multiple suppressions on a
    /// single code artifact.
    /// </summary>
    /// <remarks>
    /// <see cref="UnconditionalSuppressMessageAttribute"/> is different than
    /// <see cref="SuppressMessageAttribute"/> in that it doesn't have a
    /// <see cref="ConditionalAttribute"/>. So it is always preserved in the compiled assembly.
    /// </remarks>
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    internal sealed class UnconditionalSuppressMessageAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnconditionalSuppressMessageAttribute"/>
        /// class, specifying the category of the tool and the identifier for an analysis rule.
        /// </summary>
        /// <param name="category">The category for the attribute.</param>
        /// <param name="checkId">The identifier of the analysis rule the attribute applies to.</param>
        public UnconditionalSuppressMessageAttribute(string category, string checkId)
        {
            Category = category;
            CheckId = checkId;
        }

        /// <summary>
        /// Gets the category identifying the classification of the attribute.
        /// </summary>
        /// <remarks>
        /// The <see cref="Category"/> property describes the tool or tool analysis category
        /// for which a message suppression attribute applies.
        /// </remarks>
        public string Category { get; }

        /// <summary>
        /// Gets the identifier of the analysis tool rule to be suppressed.
        /// </summary>
        /// <remarks>
        /// Concatenated together, the <see cref="Category"/> and <see cref="CheckId"/>
        /// properties form a unique check identifier.
        /// </remarks>
        public string CheckId { get; }

        /// <summary>
        /// Gets or sets the scope of the code that is relevant for the attribute.
        /// </summary>
        /// <remarks>
        /// The Scope property is an optional argument that specifies the metadata scope for which
        /// the attribute is relevant.
        /// </remarks>
        public string? Scope { get; set; }

        /// <summary>
        /// Gets or sets a fully qualified path that represents the target of the attribute.
        /// </summary>
        /// <remarks>
        /// The <see cref="Target"/> property is an optional argument identifying the analysis target
        /// of the attribute. An example value is "System.IO.Stream.ctor():System.Void".
        /// Because it is fully qualified, it can be long, particularly for targets such as parameters.
        /// The analysis tool user interface should be capable of automatically formatting the parameter.
        /// </remarks>
        public string? Target { get; set; }

        /// <summary>
        /// Gets or sets an optional argument expanding on exclusion criteria.
        /// </summary>
        /// <remarks>
        /// The <see cref="MessageId "/> property is an optional argument that specifies additional
        /// exclusion where the literal metadata target is not sufficiently precise. For example,
        /// the <see cref="UnconditionalSuppressMessageAttribute"/> cannot be applied within a method,
        /// and it may be desirable to suppress a violation against a statement in the method that will
        /// give a rule violation, but not against all statements in the method.
        /// </remarks>
        public string? MessageId { get; set; }

        /// <summary>
        /// Gets or sets the justification for suppressing the code analysis message.
        /// </summary>
        public string? Justification { get; set; }
    }
}

#endif
