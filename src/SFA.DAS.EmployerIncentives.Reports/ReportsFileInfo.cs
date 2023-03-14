namespace SFA.DAS.EmployerIncentives.Reports
{
    public struct ReportsFileInfo
    {
        public string Name { get; private set; }
        public string Extension { get; private set; }
        public string ContentType { get; private set; }
        public string Folder { get; private set; }

        public ReportsFileInfo(string name, string extension, string contentType, string folder = "")
        {
            Name = name;
            Extension = extension;
            ContentType = contentType;
            Folder = folder;
        }
    }
}
