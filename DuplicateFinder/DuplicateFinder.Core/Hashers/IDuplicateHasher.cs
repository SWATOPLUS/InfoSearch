namespace DuplicateFinder.Core.Hashers
{
    public interface IDuplicateHasher<out T>
    {
        T[] Hash(string text);
    }
}
