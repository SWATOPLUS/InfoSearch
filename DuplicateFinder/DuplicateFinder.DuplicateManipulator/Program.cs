using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DuplicateFinder.Core;

namespace DuplicateFinder.DuplicateManipulator
{
    internal static class Program
    {
        private const string InputFile = "../../assets/sources.json";
        private const string OutputFile = "../../assets/sources-clean.json";
        private const string GroundTruthFile = "../../assets/ground_truth.tsv";

        private static void Main()
        {
            var groundTruth = DuplicateRates.FromLines(File.ReadLines(GroundTruthFile));
            var documents = DirectoryTools.ReadStringDictionary(InputFile)
                .MapValues(SourceCodeNormalizer.NormalizeContent)
                .MapValues(SourceCodeNormalizer.NormalizeSpaces)
                .MapValues(SourceCodeNormalizer.NormalizeWords)
                .MapValues(SourceCodeNormalizer.NormalizeSpaces);

            var hashes = HashTools.HashValues(documents);
            var duplicates = FindDuplicates(hashes, groundTruth);

            var result = documents.RemoveKeys(duplicates);

            DirectoryTools.SaveAsJson(result, OutputFile);
        }

        private static IReadOnlyCollection<string> FindDuplicates(
            Dictionary<ulong, string[]> hashes,
            DuplicateRates duplicateRates)
        {
            var totalErrors = 0;
            var groupCount = 0;
            var duplicates = new List<string>();

            foreach (var (_, names) in hashes)
            {
                if (names.Length < 2)
                {
                    continue;
                }

                var errors = 0;
                var parent = names.First();

                foreach (var name in names.Skip(1))
                {
                    if (duplicateRates.GetSimilarity(parent, name) - 1 < 0.001)
                    {
                        duplicates.Add(name);
                    }
                    else
                    {
                        errors++;
                        totalErrors++;
                    }
                }

                groupCount++;

                Console.WriteLine($"Group {groupCount}, size: {names.Length}, mistakes: {errors}");
            }

            Console.WriteLine($"Total errors: {totalErrors}");
            Console.WriteLine($"Total groups: {groupCount}");
            Console.WriteLine($"Total duplicates: {duplicates.Count}");

            return duplicates;
        }
    }
}