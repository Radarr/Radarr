using System;
using System.Collections.Generic;
using System.Text;

namespace NzbDrone.Core.MediaFiles.Azw
{
    public class ExtMeta
    {
        public Dictionary<uint, ulong> IdValue;
        public Dictionary<uint, string> IdString;
        public Dictionary<uint, string> IdHex;

        public ExtMeta(byte[] ext, Encoding encoding)
        {
            IdValue = new Dictionary<uint, ulong>();
            IdString = new Dictionary<uint, string>();
            IdHex = new Dictionary<uint, string>();

            var num_items = Util.GetUInt32(ext, 8);
            uint pos = 12;
            for (var i = 0; i < num_items; i++)
            {
                var id = Util.GetUInt32(ext, pos);
                var size = Util.GetUInt32(ext, pos + 4);
                if (IdMapping.Id_map_strings.ContainsKey(id))
                {
                    var a = encoding.GetString(Util.SubArray(ext, pos + 8, size - 8));

                    if (IdString.ContainsKey(id))
                    {
                        if (id == 100 || id == 517)
                        {
                            IdString[id] += "&" + a;
                        }
                        else
                        {
                            Console.WriteLine(string.Format("Meta id duplicate:{0}\nPervious:{1}  \nLatter:{2}", IdMapping.Id_map_strings[id], IdString[id], a));
                        }
                    }
                    else
                    {
                        IdString.Add(id, a);
                    }
                }
                else if (IdMapping.Id_map_values.ContainsKey(id))
                {
                    ulong a = 0;
                    switch (size)
                    {
                        case 9: a = Util.GetUInt8(ext, pos + 8); break;
                        case 10: a = Util.GetUInt16(ext, pos + 8); break;
                        case 12: a = Util.GetUInt32(ext, pos + 8); break;
                        case 16: a = Util.GetUInt64(ext, pos + 8); break;
                        default: Console.WriteLine("unexpected size:" + size); break;
                    }

                    if (IdValue.ContainsKey(id))
                    {
                        Console.WriteLine(string.Format("Meta id duplicate:{0}\nPervious:{1}  \nLatter:{2}", IdMapping.Id_map_values[id], IdValue[id], a));
                    }
                    else
                    {
                        IdValue.Add(id, a);
                    }
                }
                else if (IdMapping.Id_map_hex.ContainsKey(id))
                {
                    var a = Util.ToHexString(ext, pos + 8, size - 8);

                    if (IdHex.ContainsKey(id))
                    {
                        Console.WriteLine(string.Format("Meta id duplicate:{0}\nPervious:{1}  \nLatter:{2}", IdMapping.Id_map_hex[id], IdHex[id], a));
                    }
                    else
                    {
                        IdHex.Add(id, a);
                    }
                }
                else
                {
                    // Unknown id
                }

                pos += size;
            }
        }

        public string StringOrNull(uint key)
        {
            return IdString.TryGetValue(key, out var value) ? value : null;
        }
    }

    public class MobiHeader : Section
    {
        private readonly uint _length;
        private readonly uint _codepage;
        private readonly uint _exth_flag;

        public MobiHeader(byte[] header)
        : base("Mobi Header", header)
        {
            var mobi = Encoding.ASCII.GetString(header, 16, 4);
            if (mobi != "MOBI")
            {
                throw new AzwTagException("Invalid mobi header");
            }

            Version = Util.GetUInt32(header, 36);
            MobiType = Util.GetUInt32(header, 24);

            _codepage = Util.GetUInt32(header, 28);

            var encoding = _codepage == 65001 ? Encoding.UTF8 : CodePagesEncodingProvider.Instance.GetEncoding((int)_codepage);
            Title = encoding.GetString(header, (int)Util.GetUInt32(header, 0x54), (int)Util.GetUInt32(header, 0x58));

            _exth_flag = Util.GetUInt32(header, 0x80);
            _length = Util.GetUInt32(header, 20);
            if ((_exth_flag & 0x40) > 0)
            {
                var exth = Util.SubArray(header, _length + 16, Util.GetUInt32(header, _length + 20));
                ExtMeta = new ExtMeta(exth, encoding);
            }
            else
            {
                throw new AzwTagException("No EXTH header. Readarr cannot process this file.");
            }
        }

        public string Title { get; private set; }
        public uint Version { get; private set; }
        public uint MobiType { get; private set; }
        public ExtMeta ExtMeta { get; private set; }
    }
}
