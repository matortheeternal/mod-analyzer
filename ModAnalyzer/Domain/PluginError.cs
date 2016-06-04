namespace ModAnalyzer.Domain
{
    public class PluginError
    {
        public int Group { get; set; }
        public string Signature { get; set; }
        public int Form_Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Data { get; set; }
    }
}
