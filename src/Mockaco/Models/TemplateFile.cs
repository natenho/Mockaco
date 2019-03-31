namespace Mockaco
{
    public class TemplateFile
    {
        public string FileName { get; }
        public string Content { get; }

        public TemplateFile(string fileName, string content)
        {
            FileName = fileName;
            Content = content;
        }
    }
}