using System;
using System.Collections.Generic;
using System.Linq;
using WindowsAudioSource.Extensions;
using Xunit;

namespace WindowsAudioSource.Test
{
    public class EnumerableExtensionsTests
    {
        [Fact]
        public void FirstInGroupOrDefault_SingleValueInMatchedGroup_ReturnsValue()
        {
            var list = new List<string>
            {
                "1-one",
                "2-two",
                "3-three"
            };
            var groups = list.GroupBy(value => Convert.ToInt32(value.Split('-')[0]));

            Assert.Equal("2-two", groups.FirstInGroupOrDefault(2));
        }

        [Fact]
        public void FirstInGroupOrDefault_MultipleValuesMatchedInGroup_ReturnsFirstValue()
        {
            var list = new List<string>
            {
                "1-one",
                "2-two",
                "2-three",
                "3-four"
            };
            var groups = list.GroupBy(value => Convert.ToInt32(value.Split('-')[0]));

            Assert.Equal("2-two", groups.FirstInGroupOrDefault(2));
        }

        [Fact]
        public void FirstInGroupOrDefault_NoMatchingGroup_ReturnsDefault()
        {
            var list = new List<string>
            {
                "1-one",
                "2-two",
                "3-three"
            };
            var groups = list.GroupBy(value => Convert.ToInt32(value.Split('-')[0]));

            Assert.Equal(default(string), groups.FirstInGroupOrDefault(5));
        }

        [Fact]
        public void FirstInGroupOrDefault_EmptySequence_ReturnsDefault()
        {
            var list = new List<int>();
            var groups = list.GroupBy(value => value);

            Assert.Equal(default(int), groups.FirstInGroupOrDefault(1));
        }
    }
}
