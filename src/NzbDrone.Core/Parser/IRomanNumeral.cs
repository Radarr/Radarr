namespace NzbDrone.Core.Parser
{
    public interface IRomanNumeral
    {
        int CompareTo(object obj);
        int CompareTo(RomanNumeral other);
        bool Equals(RomanNumeral other);
        int ToInt();
        long ToLong();
        string ToString();
    }
}