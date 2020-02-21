using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Datastore.Converters
{
    public class SecondaryAlbumTypeIntConverter : JsonConverter<SecondaryAlbumType>
    {
        public override SecondaryAlbumType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var item = reader.GetInt32();
            return (SecondaryAlbumType)item;
        }

        public override void Write(Utf8JsonWriter writer, SecondaryAlbumType value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue((int)value);
        }
    }
}
