using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using WikiDownloader.GraphAnalyzerCli.Models;

namespace WikiDownloader.GraphAnalyzerCli
{
    internal static class Program
    {
        private static readonly decimal[] Deltas = {0.95m, 0.85m, 0.5m, 0.3m};

        private const int Iterations = 100;

        private const string EdgesInputFileName = "edges.output.json";
        private static  string BuildRanksOutputFileName(decimal delta) => $"ranks-{delta}.output.json";

        private static void Main(string[] args)
        {
            var json = File.ReadAllText(EdgesInputFileName);
            var pageOutputs = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(json);
            var pageInputs = pageOutputs
                .SelectMany(node => node.Value.Select(x => (Source: node.Key, Destination: x)))
                .GroupBy(x => x.Destination)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Source).ToArray());

            foreach (var delta in Deltas)
            {
                var result = BuildPageRank(delta, pageInputs, pageOutputs);
                File.WriteAllText(
                    BuildRanksOutputFileName(delta),
                    JsonConvert.SerializeObject(result, Formatting.Indented));
            }
        }

        private static PageRankResult BuildPageRank(
            decimal delta,
            Dictionary<string, string[]> inputs,
            Dictionary<string, string[]> outputs)
        {
            var pageCount = outputs.Keys.Count;
            var initialRank = 1m / pageCount;

            var pageRanks = outputs.Keys
                .ToDictionary(x => x, x => initialRank);
            

            foreach (var _ in Enumerable.Range(0, Iterations))
            {
                var oldPageRanks = pageRanks;
                pageRanks = new Dictionary<string, decimal>();

                foreach (var key in outputs.Keys)
                {
                    var result = (1m - delta) / pageCount;

                    if (!inputs.ContainsKey(key))
                    {
                        pageRanks[key] = result;

                        continue;
                    }

                    var sum = inputs[key]
                        .Sum(inputKey => oldPageRanks[inputKey] / outputs[inputKey].Length);

                    pageRanks[key] = result + delta * sum;
                }
            }

            return new PageRankResult
            {
                Iterations = Iterations,
                Delta = delta,
                Error = 1m - pageRanks.Values.Sum(),
                TopPageRanks = pageRanks
                    .OrderByDescending(x => x.Value)
                    .Take(10)
                    .ToDictionary(x => x.Key, x => x.Value),
            };
        }
    }
}
