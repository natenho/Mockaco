namespace System.IO
{
    public static class FileInfoExtensions
    {
        public static string RelativePath(this FileInfo file, DirectoryInfo directory)
        {
            Uri pathUri = new Uri(file.FullName);

            var folder = directory.FullName;

            // Folders must end in a slash
            if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                folder += Path.DirectorySeparatorChar;
            }

            Uri folderUri = new Uri(folder);

            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
