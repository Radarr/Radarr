using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Datastore.Converters
{
    public class PrimaryAlbumTypeIntConverter : JsonConverter<PrimaryAlbumType>
    {
        public override PrimaryAlbumType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var item = reader.GetInt32();
            return (PrimaryAlbumType)item;
        }

        public override void Write(Utf8JsonWriter writer, PrimaryAlbumType value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue((int)value);
        }
    }
}
