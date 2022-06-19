namespace Mockaco
{
    public interface IGlobalVariableStorage
    {
        object this[string name] { get; set; }

        void EnableWriting();

        void DisableWriting();

        void Clear();
    }
}