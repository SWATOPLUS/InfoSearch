using System;
using System.Linq;

namespace DuplicateFinder.Core.Hashers
{
    public class MinDuplicateHasher : IDuplicateHasher<ulong>
    {
        private readonly int _hashCount;
        private readonly int _gramCount;

        public MinDuplicateHasher(int hashCount, int gramCount)
        {
            _hashCount = hashCount;
            _gramCount = gramCount;
        }

        public ulong[] Hash(string text)
        {
            var words = text.Split();
            var hashes = Enumerable.Range(0, _hashCount)
                .Select(_ => ulong.MaxValue)
                .ToArray();

            foreach (var grams in words.GetNGrams(_gramCount))
            {
                var gram = string.Join(' ', grams);
                var newHashes = new ulong[_hashCount];

                foreach (var i in Enumerable.Range(0, _hashCount))
                {
                    newHashes[i] = HashTools.GetLongHash(gram, i);
                }

                hashes = hashes
                    .Zip(newHashes, Math.Min)
                    .ToArray();
            }

            return hashes;
        }
    }
}
