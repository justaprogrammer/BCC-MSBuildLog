namespace BCC.MSBuildLog
{
    public class ApplicationArguments
    {
        public string InputFile { get; set; }
        public string OutputFile { get; set; }
        public string ConfigurationFile { get; set; }
        public string CloneRoot { get; set; }
        public string OwnerRepo { get; set; }
        public string Owner { get; set; }
        public string Repo { get; set; }
        public string Hash { get; set; }
    }
}