using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace WikiDownloader.GraphAnalyzerCli.Tests
{
    public class ProgramTests
    {
        [Fact]
        public void BuildPageRank_GraphGiven_ErrorShouldBeLessThen10eMinus6()
        {
            var graph = new Dictionary<string, string[]>
            {
                ["A"] = new [] { "B" },
                ["B"] = new string[0],
            };

            var result = Program.BuildPageRank(0.95m, graph);

            Math.Abs(result.Error).Should().BeLessThan(0.000001m);
        }
    }
}
