using Bogus.DataSets;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Mockaco.Tests.Extensions
{
    public class EnumerableExtensionTest
    {
        [Theory]
        [MemberData(nameof(Data))]
        public void Select_Random_IEnumerables(List<object> source)
        {
            IEnumerable<object> enummerables = source;

            object selected = enummerables.Random();

            Assert.Contains(selected, source);
        }

        public static IEnumerable<object[]> Data()
        {
            yield return new object[] { new List<object> { 1, 2, 3 } };
            yield return new object[] { new List<object> { "a", "b", "c" } };
            yield return new object[] { new List<object> { "a1", "c2" } };
        }
    }
}
