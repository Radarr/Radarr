using System;

namespace NzbDrone.Core.MediaFiles.Azw
{
    public class Util
    {
        public static byte[] SubArray(byte[] src, ulong start, ulong length)
        {
            var r = new byte[length];
            for (ulong i = 0; i < length; i++)
            {
                r[i] = src[start + i];
            }

            return r;
        }

        public static byte[] SubArray(byte[] src, int start, int length)
        {
            var r = new byte[length];

            for (var i = 0; i < length; i++)
            {
                r[i] = src[start + i];
            }

            return r;
        }

        public static string ToHexString(byte[] src, uint start, uint length)
        {
            //https://stackoverflow.com/a/14333437/48700
            var c = new char[length * 2];
            int b;
            for (var i = 0; i < length; i++)
            {
                b = src[start + i] >> 4;
                c[i * 2] = (char)(55 + b + (((b - 10) >> 31) & -7));
                b = src[start + i] & 0xF;
                c[(i * 2) + 1] = (char)(55 + b + (((b - 10) >> 31) & -7));
            }

            return new string(c);
        }

        public static ulong GetUInt64(byte[] src, ulong start)
        {
            var t = SubArray(src, start, 8);
            Array.Reverse(t);
            return BitConverter.ToUInt64(t, 0);
        }

        //big edian handle:
        public static uint GetUInt32(byte[] src, ulong start)
        {
            var t = SubArray(src, start, 4);
            Array.Reverse(t);
            return BitConverter.ToUInt32(t, 0);
        }

        public static ushort GetUInt16(byte[] src, ulong start)
        {
            var t = SubArray(src, start, 2);
            Array.Reverse(t);
            return BitConverter.ToUInt16(t, 0);
        }

        public static byte GetUInt8(byte[] src, ulong start)
        {
            return src[start];
        }
    }

    [Serializable]
    public class AzwTagException : Exception
    {
        public AzwTagException(string message)
        : base(message)
        {
        }

        protected AzwTagException(System.Runtime.Serialization.SerializationInfo info,
                                  System.Runtime.Serialization.StreamingContext context)
        : base(info, context)
        {
        }
    }
}
