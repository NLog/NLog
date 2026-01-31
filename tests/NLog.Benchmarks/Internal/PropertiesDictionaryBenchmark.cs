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

namespace NLog.Benchmarks
{
    using BenchmarkDotNet.Attributes;
    using NLog.Internal;

    public class PropertiesDictionaryBenchmark
    {
        [Benchmark]
        public void CreateOneProperty()
        {
            var properties = new PropertiesDictionary();
            properties["Hello"] = "World";
        }

        [Benchmark]
        public void CreateTwoProperty()
        {
            var properties = new PropertiesDictionary();
            properties["Hello1"] = "World1";
            properties["Hello2"] = "World2";
        }

        [Benchmark]
        public void CreateThreeProperty()
        {
            var properties = new PropertiesDictionary();
            properties["Hello1"] = "World1";
            properties["Hello2"] = "World2";
            properties["Hello3"] = "World3";
        }

        [Benchmark]
        public void CreateFourProperty()
        {
            var properties = new PropertiesDictionary();
            properties["Hello1"] = "World1";
            properties["Hello2"] = "World2";
            properties["Hello3"] = "World3";
            properties["Hello4"] = "World4";
        }

        [Benchmark]
        public void CreateFiveProperty()
        {
            var properties = new PropertiesDictionary();
            properties["Hello1"] = "World1";
            properties["Hello2"] = "World2";
            properties["Hello3"] = "World3";
            properties["Hello4"] = "World4";
            properties["Hello5"] = "World5";
        }

        [Benchmark]
        public void LookupOneProperty()
        {
            var properties = new PropertiesDictionary();
            properties["Hello"] = "World";
            for (int i = 0; i < 1000; i++)
            {
                if (!properties.TryGetValue("Hello", out var _))
                    break;
            }
        }

        [Benchmark]
        public void LookupTwoProperty()
        {
            var properties = new PropertiesDictionary();
            properties["Hello1"] = "World1";
            properties["Hello2"] = "World2";
            for (int i = 0; i < 1000; i++)
            {
                if (!properties.TryGetValue("Hello1", out var _))
                    break;
                if (!properties.TryGetValue("Hello2", out var _))
                    break;
            }
        }

        [Benchmark]
        public void LookupThreeProperty()
        {
            var properties = new PropertiesDictionary();
            properties["Hello1"] = "World1";
            properties["Hello2"] = "World2";
            properties["Hello3"] = "World3";
            for (int i = 0; i < 1000; i++)
            {
                if (!properties.TryGetValue("Hello1", out var _))
                    break;
                if (!properties.TryGetValue("Hello2", out var _))
                    break;
                if (!properties.TryGetValue("Hello3", out var _))
                    break;
            }
        }

        [Benchmark]
        public void LookupFourProperty()
        {
            var properties = new PropertiesDictionary();
            properties["Hello1"] = "World1";
            properties["Hello2"] = "World2";
            properties["Hello3"] = "World3";
            properties["Hello4"] = "World4";
            for (int i = 0; i < 1000; i++)
            {
                if (!properties.TryGetValue("Hello1", out var _))
                    break;
                if (!properties.TryGetValue("Hello2", out var _))
                    break;
                if (!properties.TryGetValue("Hello3", out var _))
                    break;
                if (!properties.TryGetValue("Hello4", out var _))
                    break;
            }
        }

        [Benchmark]
        public void LookupFiveProperty()
        {
            var properties = new PropertiesDictionary();
            properties["Hello1"] = "World1";
            properties["Hello2"] = "World2";
            properties["Hello3"] = "World3";
            properties["Hello4"] = "World4";
            properties["Hello5"] = "World5";
            for (int i = 0; i < 1000; i++)
            {
                if (!properties.TryGetValue("Hello1", out var _))
                    break;
                if (!properties.TryGetValue("Hello2", out var _))
                    break;
                if (!properties.TryGetValue("Hello3", out var _))
                    break;
                if (!properties.TryGetValue("Hello4", out var _))
                    break;
                if (!properties.TryGetValue("Hello5", out var _))
                    break;
            }
        }

        [Benchmark]
        public void NotFoundLookupOneProperty()
        {
            var properties = new PropertiesDictionary();
            properties["Hello"] = "World";
            for (int i = 0; i < 1000; i++)
            {
                if (properties.TryGetValue("Hello6", out var _))
                    break;
            }
        }

        [Benchmark]
        public void NotFoundLookupTwoProperty()
        {
            var properties = new PropertiesDictionary();
            properties["Hello1"] = "World1";
            properties["Hello2"] = "World2";
            for (int i = 0; i < 1000; i++)
            {
                if (properties.TryGetValue("Hello6", out var _))
                    break;
            }
        }

        [Benchmark]
        public void NotFoundLookupThreeProperty()
        {
            var properties = new PropertiesDictionary();
            properties["Hello1"] = "World1";
            properties["Hello2"] = "World2";
            properties["Hello3"] = "World3";
            for (int i = 0; i < 1000; i++)
            {
                if (properties.TryGetValue("Hello6", out var _))
                    break;
            }
        }

        [Benchmark]
        public void NotFoundLookupFourProperty()
        {
            var properties = new PropertiesDictionary();
            properties["Hello1"] = "World1";
            properties["Hello2"] = "World2";
            properties["Hello3"] = "World3";
            properties["Hello4"] = "World4";
            for (int i = 0; i < 1000; i++)
            {
                if (properties.TryGetValue("Hello6", out var _))
                    break;
            }
        }

        [Benchmark]
        public void NotFoundLookupFiveProperty()
        {
            var properties = new PropertiesDictionary();
            properties["Hello1"] = "World1";
            properties["Hello2"] = "World2";
            properties["Hello3"] = "World3";
            properties["Hello4"] = "World4";
            properties["Hello5"] = "World5";
            for (int i = 0; i < 1000; i++)
            {
                if (properties.TryGetValue("Hello6", out var _))
                    break;
            }
        }

        [Benchmark]
        public void EnumerateOneProperty()
        {
            var properties = new PropertiesDictionary();
            properties["Hello"] = "World";
            for (int i = 0; i < 1000; i++)
            {
                using (var propertyEnumerator = properties.GetPropertyEnumerator())
                {
                    while (propertyEnumerator.MoveNext())
                    {
                        if (propertyEnumerator.Current.Key is null)
                            return;
                    }
                }
            }
        }

        [Benchmark]
        public void EnumerateTwoProperty()
        {
            var properties = new PropertiesDictionary();
            properties["Hello1"] = "World1";
            properties["Hello2"] = "World2";
            for (int i = 0; i < 1000; i++)
            {
                using (var propertyEnumerator = properties.GetPropertyEnumerator())
                {
                    while (propertyEnumerator.MoveNext())
                    {
                        if (propertyEnumerator.Current.Key is null)
                            return;
                    }
                }
            }
        }

        [Benchmark]
        public void EnumerateThreeProperty()
        {
            var properties = new PropertiesDictionary();
            properties["Hello1"] = "World1";
            properties["Hello2"] = "World2";
            properties["Hello3"] = "World3";
            for (int i = 0; i < 1000; i++)
            {
                using (var propertyEnumerator = properties.GetPropertyEnumerator())
                {
                    while (propertyEnumerator.MoveNext())
                    {
                        if (propertyEnumerator.Current.Key is null)
                            return;
                    }
                }
            }
        }

        [Benchmark]
        public void EnumerateFourProperty()
        {
            var properties = new PropertiesDictionary();
            properties["Hello1"] = "World1";
            properties["Hello2"] = "World2";
            properties["Hello3"] = "World3";
            properties["Hello4"] = "World4";
            for (int i = 0; i < 1000; i++)
            {
                using (var propertyEnumerator = properties.GetPropertyEnumerator())
                {
                    while (propertyEnumerator.MoveNext())
                    {
                        if (propertyEnumerator.Current.Key is null)
                            return;
                    }
                }
            }
        }

        [Benchmark]
        public void EnumerateFiveProperty()
        {
            var properties = new PropertiesDictionary();
            properties["Hello1"] = "World1";
            properties["Hello2"] = "World2";
            properties["Hello3"] = "World3";
            properties["Hello4"] = "World4";
            properties["Hello5"] = "World5";
            for (int i = 0; i < 1000; i++)
            {
                using (var propertyEnumerator = properties.GetPropertyEnumerator())
                {
                    while (propertyEnumerator.MoveNext())
                    {
                        if (propertyEnumerator.Current.Key is null)
                            return;
                    }
                }
            }
        }
    }
}
