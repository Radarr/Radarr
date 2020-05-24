using System.IO;
using System.Threading.Tasks;

namespace NzbDrone.Common.Extensions
{
    public static class StreamExtensions
    {
        public static async Task<byte[]> ToBytes(this Stream input)
        {
            var buffer = new byte[16 * 1024];
            using (var ms = new MemoryStream())
            {
                int read;
                while ((read = await input.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await ms.WriteAsync(buffer, 0, read);
                }

                return ms.ToArray();
            }
        }
    }
}
