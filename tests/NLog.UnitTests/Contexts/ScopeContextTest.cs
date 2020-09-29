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

namespace NLog.UnitTests.Contexts
{
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class ScopeContextTest
    {
        [Fact]
        public void PushPropertyCaseInsensitiveTest()
        {
            // Arrange
            ScopeContext.Clear();
            var expectedValue = "World";
            Dictionary<string, object> allProperties = null;
            var success = false;
            object value;

            // Act
            using (ScopeContext.PushProperty("HELLO", expectedValue))
            {
                success = ScopeContext.TryLookupProperty("hello", out value);
                allProperties = ScopeContext.GetAllProperties().ToDictionary(x => x.Key, x => x.Value);
            }
            var failed = ScopeContext.TryLookupProperty("hello", out var _);

            // Assert
            Assert.True(success);
            Assert.Equal(expectedValue, value);
            Assert.Single(allProperties);
            Assert.Equal(expectedValue, allProperties["HELLO"]);
            Assert.False(failed);
        }

        [Fact]
        public void PushPropertyNestedTest()
        {
            // Arrange
            ScopeContext.Clear();
            var expectedString = "World";
            var expectedGuid = System.Guid.NewGuid();
            Dictionary<string, object> allProperties = null;
            object stringValueLookup1 = null;
            object stringValueLookup2 = null;
            bool stringValueLookup3 = false;
            object guidValueLookup1 = null;
            bool guidValueLookup2 = false;
            bool guidValueLookup3 = false;

            // Act
            using (ScopeContext.PushProperty("Hello", expectedString))
            {
                using (ScopeContext.PushProperty("RequestId", expectedGuid))
                {
                    ScopeContext.TryLookupProperty("Hello", out stringValueLookup1);
                    ScopeContext.TryLookupProperty("RequestId", out guidValueLookup1);
                    allProperties = ScopeContext.GetAllProperties().ToDictionary(x => x.Key, x => x.Value);
                }

                ScopeContext.TryLookupProperty("Hello", out stringValueLookup2);
                guidValueLookup2 = ScopeContext.TryLookupProperty("RequestId", out var _);
            }
            guidValueLookup3 = ScopeContext.TryLookupProperty("RequestId", out var _);
            stringValueLookup3 = ScopeContext.TryLookupProperty("Hello", out var _);

            // Assert
            Assert.Equal(2, allProperties.Count);
            Assert.Equal(expectedString, allProperties["Hello"]);
            Assert.Equal(expectedGuid, allProperties["RequestId"]);
            Assert.Equal(expectedString, stringValueLookup1);
            Assert.Equal(expectedString, stringValueLookup2);
            Assert.Equal(expectedGuid, guidValueLookup1);
            Assert.False(guidValueLookup2);
            Assert.False(guidValueLookup3);
            Assert.False(guidValueLookup3);
            Assert.False(stringValueLookup3);
        }

#if !NET3_5 && !NET4_0
        [Fact]
        public void PushOperationPropertiesTest()
        {
            // Arrange
            ScopeContext.Clear();
            var expectedString = "World";
            var expectedGuid = System.Guid.NewGuid();
            var expectedProperties = new[] { new KeyValuePair<string, object>("Hello", expectedString), new KeyValuePair<string, object>("RequestId", expectedGuid) };
            var expectedOperationState = "First Push";
            Dictionary<string, object> allProperties = null;
            object[] allOperationStates = null;
            object stringValueLookup = null;

            // Act
            using (ScopeContext.PushProperty("Hello", "People"))
            {
                using (ScopeContext.PushOperationProperties(expectedOperationState, expectedProperties))
                {
                    allOperationStates = ScopeContext.GetAllOperationStates();
                    allProperties = ScopeContext.GetAllProperties().ToDictionary(x => x.Key, x => x.Value);
                }
                ScopeContext.TryLookupProperty("Hello", out stringValueLookup);
            }

            // Assert
            Assert.Equal(2, allProperties.Count);
            Assert.Equal(expectedString, allProperties["Hello"]);
            Assert.Equal(expectedGuid, allProperties["RequestId"]);
            Assert.Single(allOperationStates);
            Assert.Equal(expectedOperationState, allOperationStates[0]);
            Assert.Equal("People", stringValueLookup);
        }
#endif


        [Fact]
        public void PushOperationStateTest()
        {
            // Arrange
            ScopeContext.Clear();
            var expectedOperationState = "First Push";
            object topOperationState = null;
            object[] allOperationStates = null;

            // Act
            using (ScopeContext.PushOperationState(expectedOperationState))
            {
                topOperationState = ScopeContext.PeekOperationState();
                allOperationStates = ScopeContext.GetAllOperationStates();
            }
            var failed = ScopeContext.PeekOperationState() != null;

            // Assert
            Assert.Equal(expectedOperationState, topOperationState);
            Assert.Single(allOperationStates);
            Assert.Equal(expectedOperationState, allOperationStates[0]);
            Assert.False(failed);
        }

        [Fact]
        public void PushNestedOperationStateTest()
        {
            // Arrange
            ScopeContext.Clear();
            var expectedOperationState1 = "First Push";
            var expectedOperationState2 = System.Guid.NewGuid();
            object topOperationState1 = null;
            object topOperationState2 = null;
            object[] allOperationStates = null;

            // Act
            using (ScopeContext.PushOperationState(expectedOperationState1))
            {
                topOperationState1 = ScopeContext.PeekOperationState();
                using (ScopeContext.PushOperationState(expectedOperationState2))
                {
                    topOperationState2 = ScopeContext.PeekOperationState();
                    allOperationStates = ScopeContext.GetAllOperationStates();
                }                   
            }
            var failed = ScopeContext.PeekOperationState() != null;

            // Assert
            Assert.Equal(expectedOperationState1, topOperationState1);
            Assert.Equal(expectedOperationState2, topOperationState2);
            Assert.Equal(2, allOperationStates.Length);
            Assert.Equal(expectedOperationState2, allOperationStates[0]);
            Assert.Equal(expectedOperationState1, allOperationStates[1]);
            Assert.False(failed);
        }

        [Fact]
        public void ClearScopeContextTest()
        {
            // Arrange
            ScopeContext.Clear();
            var expectedOperationState = "First Push";
            var expectedString = "World";
            var expectedGuid = System.Guid.NewGuid();
            object[] allOperationStates1 = null;
            object[] allOperationStates2 = null;
            object stringValueLookup1 = null;
            object stringValueLookup2 = null;

            // Act
            using (ScopeContext.PushProperty("Hello", expectedString))
            {
                using (ScopeContext.PushProperty("RequestId", expectedGuid))
                {
                    using (ScopeContext.PushOperationState(expectedOperationState))
                    {
                        ScopeContext.Clear();
                        allOperationStates1 = ScopeContext.GetAllOperationStates();
                        ScopeContext.TryLookupProperty("Hello", out stringValueLookup1);
                    }
                }

                // Original scope was restored on dispose, verify expected behavior
                allOperationStates2 = ScopeContext.GetAllOperationStates();
                ScopeContext.TryLookupProperty("Hello", out stringValueLookup2);
            }

            // Assert
            Assert.Null(stringValueLookup1);
            Assert.Equal(expectedString, stringValueLookup2);
            Assert.Empty(allOperationStates1);
            Assert.Empty(allOperationStates2);
        }

        [Fact]
        public void LegacyNdlcPopShouldNotAffectProperties1()
        {
            // Arrange
            ScopeContext.Clear();
            var expectedValue = "World";
            var success = false;
            object propertyValue;

            // Act
            using (ScopeContext.PushProperty("Hello", expectedValue))
            {
                NestedDiagnosticsLogicalContext.PopObject();    // Should not pop anything (skip legacy mode)
                success = ScopeContext.TryLookupProperty("Hello", out propertyValue);
            }

            // Assert
            Assert.True(success);
            Assert.Equal(expectedValue, propertyValue);
        }

        [Fact]
        public void LegacyNdlcPopShouldNotAffectProperties2()
        {
            // Arrange
            ScopeContext.Clear();
            var expectedValue = "World";
            var expectedOperationState = "First Push";
            var success = false;
            object propertyValue;
            object operationState;

            // Act
            using (ScopeContext.PushProperty("Hello", expectedValue))
            {
                ScopeContext.PushOperationState(expectedOperationState);
                operationState = NestedDiagnosticsLogicalContext.PopObject();    // Should only pop active scope (skip legacy mode)
                success = ScopeContext.TryLookupProperty("Hello", out propertyValue);
            }

            // Assert
            Assert.True(success);
            Assert.Equal(expectedValue, propertyValue);
            Assert.Equal(expectedOperationState, operationState);
        }

        [Fact]
        public void LegacyNdlcPopShouldNotAffectProperties3()
        {
            // Arrange
            ScopeContext.Clear();
            var expectedValue1 = "World";
            var expectedValue2 = System.Guid.NewGuid();
            var expectedOperationState1 = "First Push";
            var expectedOperationState2 = System.Guid.NewGuid();
            var success1 = false;
            var success2 = false;
            object propertyValue1;
            object propertyValue2;
            object operationState1;
            object operationState2;

            // Act
            using (ScopeContext.PushProperty("Hello", expectedValue1))
            {
                ScopeContext.PushOperationState(expectedOperationState1);
                ScopeContext.PushOperationState(expectedOperationState2);
                using (ScopeContext.PushProperty("RequestId", expectedValue2))
                {
                    operationState2 = NestedDiagnosticsLogicalContext.PopObject();    // Evil pop where it should leave properties alone (Legacy mode)
                    operationState1 = NestedDiagnosticsLogicalContext.PopObject();    // Evil pop where it should leave properties alone (Legacy mode)

                    success1 = ScopeContext.TryLookupProperty("Hello", out propertyValue1);
                    success2 = ScopeContext.TryLookupProperty("RequestId", out propertyValue2);
                }
            }

            // Assert
            Assert.True(success1);
            Assert.True(success2);
            Assert.Equal(expectedValue1, propertyValue1);
            Assert.Equal(expectedValue2, propertyValue2);
            Assert.Equal(expectedOperationState1, operationState1);
            Assert.Equal(expectedOperationState2, operationState2);
        }

        [Fact]
        public void LegacyNdlcClearShouldNotAffectProperties1()
        {
            // Arrange
            ScopeContext.Clear();
            var expectedValue = "World";
            var success = false;
            object propertyValue;

            // Act
            using (ScopeContext.PushProperty("Hello", expectedValue))
            {
                NestedDiagnosticsLogicalContext.Clear();    // Should not clear anything (skip legacy mode)
                success = ScopeContext.TryLookupProperty("Hello", out propertyValue);
            }

            // Assert
            Assert.True(success);
            Assert.Equal(expectedValue, propertyValue);
        }

        [Fact]
        public void LegacyNdlcClearShouldNotAffectProperties2()
        {
            // Arrange
            ScopeContext.Clear();
            var expectedValue = "World";
            var expectedOperationState = "First Push";
            var success = false;
            object propertyValue;

            // Act
            using (ScopeContext.PushProperty("Hello", expectedValue))
            {
                ScopeContext.PushOperationState(expectedOperationState);
                NestedDiagnosticsLogicalContext.Clear();    // Should not clear properties (Legacy mode)
                success = ScopeContext.TryLookupProperty("Hello", out propertyValue);
            }

            // Assert
            Assert.True(success);
            Assert.Equal(expectedValue, propertyValue);
        }

        [Fact]
        public void LegacyMdlcClearShouldNotAffectStackValues1()
        {
            // Arrange
            ScopeContext.Clear();
            var expectedOperationState = "First Push";
            object[] allOperationStates = null;

            // Act
            using (ScopeContext.PushOperationState(expectedOperationState))
            {
                MappedDiagnosticsLogicalContext.Clear();    // Should not clear anything (skip legacy mode)
                allOperationStates = ScopeContext.GetAllOperationStates();
            }

            // Assert
            Assert.Single(allOperationStates);
            Assert.Equal(expectedOperationState, allOperationStates[0]);
        }

        [Fact]
        public void LegacyMdlcClearShouldNotAffectStackValues2()
        {
            // Arrange
            ScopeContext.Clear();
            var expectedValue = "World";
            var expectedOperationState = "First Push";
            object[] allOperationStates = null;

            // Act
            using (ScopeContext.PushOperationState(expectedOperationState))
            {
                ScopeContext.PushProperty("Hello", expectedValue);
                MappedDiagnosticsLogicalContext.Clear();    // Should not clear stack (Legacy mode)
                allOperationStates = ScopeContext.GetAllOperationStates();
            }

            // Assert
            Assert.Single(allOperationStates);
            Assert.Equal(expectedOperationState, allOperationStates[0]);
        }

        [Fact]
        public void LegacyMdlcRemoveShouldNotAffectStackValues1()
        {
            // Arrange
            ScopeContext.Clear();
            var expectedOperationState = "First Push";
            object[] allOperationStates = null;

            // Act
            using (ScopeContext.PushOperationState(expectedOperationState))
            {
                MappedDiagnosticsLogicalContext.Remove("Hello");    // Should not remove anything (skip legacy mode)
                allOperationStates = ScopeContext.GetAllOperationStates();
            }

            // Assert
            Assert.Single(allOperationStates);
            Assert.Equal(expectedOperationState, allOperationStates[0]);
        }

        [Fact]
        public void LegacyMdlcRemoveShouldNotAffectStackValues2()
        {
            // Arrange
            ScopeContext.Clear();
            var expectedValue1 = "World";
            var expectedValue2 = System.Guid.NewGuid();
            var expectedOperationState1 = "First Push";
            var expectedOperationState2 = System.Guid.NewGuid();
            object propertyValue1;
            object propertyValue2;
            object[] allOperationStates = null;
            var success1 = false;
            var success2 = false;

            // Act
            using (ScopeContext.PushOperationState(expectedOperationState1))
            {
                using (ScopeContext.PushProperty("Hello", expectedValue1))
                {
                    using (ScopeContext.PushOperationState(expectedOperationState2))
                    {
                        ScopeContext.PushProperty("RequestId", expectedValue2);
                        MappedDiagnosticsLogicalContext.Remove("RequestId");    // Should not change stack (Legacy mode)
                        allOperationStates = ScopeContext.GetAllOperationStates();

                        success1 = ScopeContext.TryLookupProperty("Hello", out propertyValue1);
                        success2 = ScopeContext.TryLookupProperty("RequestId", out propertyValue2);
                    }
                }
            }

            // Assert
            Assert.Equal(2, allOperationStates.Length);
            Assert.Equal(expectedOperationState2, allOperationStates[0]);
            Assert.Equal(expectedOperationState1, allOperationStates[1]);
            Assert.True(success1);
            Assert.False(success2);
            Assert.Equal(expectedValue1, propertyValue1);
            Assert.Null(propertyValue2);
        }

        [Fact]
        public void LegacyMdlcSetShouldNotAffectStackValues1()
        {
            // Arrange
            ScopeContext.Clear();
            var expectedValue = "World";
            var expectedOperationState = "First Push";
            object propertyValue;
            object[] allOperationStates = null;
            var success = false;

            // Act
            using (ScopeContext.PushOperationState(expectedOperationState))
            {
                MappedDiagnosticsLogicalContext.Set("Hello", expectedValue);    // Skip legacy mode (normal property push)
                success = ScopeContext.TryLookupProperty("Hello", out propertyValue);
                allOperationStates = ScopeContext.GetAllOperationStates();
            }

            // Assert
            Assert.Single(allOperationStates);
            Assert.Equal(expectedOperationState, allOperationStates[0]);
            Assert.True(success);
            Assert.Equal(expectedValue, propertyValue);
        }

        [Fact]
        public void LegacyMdlcSetShouldNotAffectStackValues2()
        {
            // Arrange
            ScopeContext.Clear();
            var expectedValue = "World";
            var expectedOperationState = "First Push";
            object propertyValue;
            object[] allOperationStates = null;
            var success = false;

            // Act
            using (ScopeContext.PushOperationState(expectedOperationState))
            {
                using (ScopeContext.PushProperty("Hello", expectedValue))
                {
                    MappedDiagnosticsLogicalContext.Set("Hello", expectedValue);    // Skip legacy mode (ignore when same value)
                    success = ScopeContext.TryLookupProperty("Hello", out propertyValue);
                    allOperationStates = ScopeContext.GetAllOperationStates();
                }
            }

            // Assert
            Assert.Single(allOperationStates);
            Assert.Equal(expectedOperationState, allOperationStates[0]);
            Assert.True(success);
            Assert.Equal(expectedValue, propertyValue);
        }

        [Fact]
        public void LegacyMdlcSetShouldNotAffectStackValues3()
        {
            // Arrange
            ScopeContext.Clear();
            var expectedValue = "Bob";
            var expectedOperationState = "First Push";
            object propertyValue1;
            object propertyValue2;
            object[] allOperationStates = null;
            var success1 = false;
            var success2 = false;

            // Act
            using (ScopeContext.PushOperationState(expectedOperationState))
            {
                using (ScopeContext.PushProperty("Hello", "World"))
                {
                    MappedDiagnosticsLogicalContext.Set("Hello", expectedValue);    // Enter legacy mode (need to overwrite)
                    success1 = ScopeContext.TryLookupProperty("Hello", out propertyValue1);
                    allOperationStates = ScopeContext.GetAllOperationStates();
                }

                success2 = ScopeContext.TryLookupProperty("Hello", out propertyValue2);
            }

            // Assert
            Assert.Single(allOperationStates);
            Assert.Equal(expectedOperationState, allOperationStates[0]);
            Assert.True(success1);
            Assert.Equal(expectedValue, propertyValue1);
            Assert.False(success2);
            Assert.Null(propertyValue2);
        }
    }
}
