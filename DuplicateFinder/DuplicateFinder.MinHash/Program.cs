using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DuplicateFinder.Core;
using DuplicateFinder.Core.Analyzers;
using DuplicateFinder.Core.Hashers;

namespace DuplicateFinder.MinHash
{
    internal static class Program
    {
        private const string InputFile = "../../assets/sources-clean.json";
        private const string OutputFile = "../../assets/compare-result-space.json";
        private const string GroundTruthFile = "../../assets/ground_truth.tsv";

        private static void Main()
        {
            var documents = DirectoryTools.ReadStringDictionary(InputFile);
            var groundTruth = DuplicateRates.FromLines(File.ReadLines(GroundTruthFile), documents.Keys);

            var result = new
            { 
                Min1 = ProcessMinHash(groundTruth, documents, 1),
                Min5 = ProcessMinHash(groundTruth, documents, 5),
                Min10 = ProcessMinHash(groundTruth, documents, 10),
                Min30 = ProcessMinHash(groundTruth, documents, 30),
                Min50 = ProcessMinHash(groundTruth, documents, 50),
                Min100 = ProcessMinHash(groundTruth, documents, 100),
                Sim8 = ProcessSimHash(groundTruth, documents, 8 / 8),
                Sim16 = ProcessSimHash(groundTruth, documents, 16 / 8),
                Sim64 = ProcessSimHash(groundTruth, documents, 64 / 8),
                Sim128 = ProcessSimHash(groundTruth, documents, 128 / 8),
                Sim256 = ProcessSimHash(groundTruth, documents, 256 / 8),
            };

            DirectoryTools.SaveAsJson(result, OutputFile);
        }

        private static SequenceStats ProcessSimHash(
            DuplicateRates groundTruth,
            Dictionary<string, string> documents,
            int bytesCount)
        {
            Console.WriteLine("Sim: " + bytesCount * 8 + " " + DateTime.Now);

            var analyzer = new HashDuplicateAnalyzer<byte>(new SimDuplicateHasher(bytesCount, 5));

            return Process(groundTruth, documents, analyzer);
        }

        private static SequenceStats ProcessMinHash(
            DuplicateRates groundTruth,
            Dictionary<string, string> documents,
            int hashCount)
        {
            Console.WriteLine("Min: " + hashCount + " " + DateTime.Now);

            var analyzer = new HashDuplicateAnalyzer<ulong>(new MinDuplicateHasher(hashCount, 5));

            return Process(groundTruth, documents, analyzer);
        }

        private static SequenceStats Process(
            DuplicateRates groundTruth,
            Dictionary<string, string> documents,
            IDuplicateAnalyzer analyzer)
        {
            var sw = new Stopwatch();
            sw.Start();

            foreach (var (name, text) in documents)
            {
                analyzer.AddText(name, text);
            }

            var duplicates = analyzer.GetAllDuplicates();

            var stats = StatisticsExtensions.GetNGDcg10Stats(groundTruth.Data, duplicates.Data);
            
            Console.WriteLine("Done in" + sw.Elapsed);

            return stats;
        }
    }
}