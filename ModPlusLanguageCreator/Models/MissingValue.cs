namespace ModPlusLanguageCreator.Models
{
    public class MissingValue
    {
        public MissingValue(string nodeName, string name)
        {
            NodeName = nodeName;
            Name = name;
        }
        public string NodeName { get; set; }
        public string Name { get; set; }
    }
}
