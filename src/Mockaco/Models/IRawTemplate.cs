namespace Mockaco
{
    public interface IRawTemplate
    {
        string Content { get; }

        string Name { get; }

        string Hash { get; }
    }
}