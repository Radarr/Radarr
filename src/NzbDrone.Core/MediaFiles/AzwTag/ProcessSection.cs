using System.Text;

namespace NzbDrone.Core.MediaFiles.Azw
{
    public class Section
    {
        public string Type;
        public byte[] Raw;
        public string Comment = "";

        public Section(byte[] raw)
        {
            Raw = raw;
            if (raw.Length < 4)
            {
                Type = "Empty Section";
                return;
            }

            Type = Encoding.ASCII.GetString(raw, 0, 4);

            switch (Type)
            {
                case "??\r\n": Type = "End Of File"; break;
                case "?6?\t": Type = "Place Holder"; break;
                case "\0\0\0\0": Type = "Empty Section0"; break;
            }
        }

        public Section(Section s)
        {
            Type = s.Type;
            Raw = s.Raw;
        }

        public Section(string type, byte[] raw)
        {
            Type = type;
            Raw = raw;
        }

        public virtual int GetSize()
        {
            return Raw.Length;
        }
    }
}
