using System.Collections;
using System.Linq;

namespace DuplicateFinder.Core.Hashers
{
    public class SimDuplicateHasher : IDuplicateHasher<byte>
    {
        private readonly int _byteCount;
        private readonly int _gramCount;

        public SimDuplicateHasher(int byteCount, int gramCount)
        {
            _byteCount = byteCount;
            _gramCount = gramCount;
        }

        public byte[] Hash(string text)
        {
            var words = text.Split();
            var result = new int[_byteCount * 8];

            foreach (var grams in words.GetNGrams(_gramCount))
            {
                var gram = string.Join(' ', grams);
                var hash = new BitArray(HashTools.GetByteHash(gram, _byteCount));

                for (var i = 0; i < hash.Length; i++)
                {
                    if (hash[i])
                    {
                        result[i]++;
                    }
                    else
                    {
                        result[i]--;
                    }
                }
            }

            var bits = result
                .Select(x => x > 0)
                .ToArray();

            var bytes = new byte[_byteCount];

            new BitArray(bits).CopyTo(bytes, 0);

            return bytes;
        }
    }
}
