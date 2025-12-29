using System;
using System.Text;

namespace NzbDrone.Common
{
    public static class HashUtil
    {
        public static string CalculateCrc(string input)
        {
            var mCrc = 0xffffffff;
            var bytes = Encoding.UTF8.GetBytes(input);
            foreach (var myByte in bytes)
            {
                mCrc ^= (uint)myByte << 24;
                for (var i = 0; i < 8; i++)
                {
                    if ((Convert.ToUInt32(mCrc) & 0x80000000) == 0x80000000)
                    {
                        mCrc = (mCrc << 1) ^ 0x04C11DB7;
                    }
                    else
                    {
                        mCrc <<= 1;
                    }
                }
            }

            return $"{mCrc:x8}";
        }

        public static string AnonymousToken()
        {
            // Machine fingerprinting removed for privacy - return non-identifying placeholder
            return "anonymous";
        }
    }
}
