using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Datastore.Converters
{
    public class ReleaseStatusIntConverter : JsonConverter<ReleaseStatus>
    {
        public override ReleaseStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var item = reader.GetInt32();
            return (ReleaseStatus)item;
        }

        public override void Write(Utf8JsonWriter writer, ReleaseStatus value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue((int)value);
        }
    }
}
