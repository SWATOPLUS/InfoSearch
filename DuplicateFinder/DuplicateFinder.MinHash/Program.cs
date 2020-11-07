using System.IO;
using DuplicateFinder.Core;

namespace DuplicateFinder.MinHash
{
    internal static class Program
    {
        private const string InputFile = "../../assets/sources-clean.json";
        private const string OutputFile = "../../assets/compare-result.json";
        private const string GroundTruthFile = "../../assets/ground_truth.tsv";

        private static void Main()
        {
            var documents = DirectoryTools.ReadStringDictionary(InputFile);
            var groundTruth = DuplicateRates.FromLines(File.ReadLines(GroundTruthFile), documents.Keys);
            var size = documents.Keys.Count;
            var duplicatesMinHash1 = DuplicationUtils.FindDuplicatesByMinHash(documents, 1);
            var duplicatesMinHash5 = DuplicationUtils.FindDuplicatesByMinHash(documents, 5);
            var duplicatesMinHash10 = DuplicationUtils.FindDuplicatesByMinHash(documents, 10);
            var duplicatesSimHash256 = DuplicationUtils.FindDuplicatesBySimHash(documents, 256 / 8);

            var result = new
            {
                DummyRate = DuplicateRates.Compare(groundTruth, DuplicateRates.Dummy, size),
                Min1 = DuplicateRates.Compare(groundTruth, duplicatesMinHash1, size),
                Min5 = DuplicateRates.Compare(groundTruth, duplicatesMinHash5, size),
                Min10 = DuplicateRates.Compare(groundTruth, duplicatesMinHash10, size),
                Sim256 = DuplicateRates.Compare(groundTruth, duplicatesSimHash256, size),
            };

            DirectoryTools.SaveAsJson(result, OutputFile);
        }


    }
}