namespace NzbDrone.Core.Parser
{
    public class IsoCountry
    {
        public string TwoLetterCode { get; set; }
        public string Name { get; set; }

        public IsoCountry(string twoLetterCode, string name)
        {
            TwoLetterCode = twoLetterCode;
            Name = name;
        }
    }
}
