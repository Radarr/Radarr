using System.IO;
using Nancy;

namespace Radarr.Http
{
    public class ByteArrayResponse : Response
    {
        public ByteArrayResponse(byte[] body, string contentType)
        {
            ContentType = contentType;

            Contents = stream =>
            {
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write(body);
                }
            };
        }
    }
}
