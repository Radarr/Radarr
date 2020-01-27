﻿using System.Data;
using System.Text.Json;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Reflection;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Datastore.Converters
{
    public class CommandConverter : EmbeddedDocumentConverter<Command>
    {
        public override Command Parse(object value)
        {
            var stringValue = (string)value;

            if (stringValue.IsNullOrWhiteSpace())
            {
                return null;
            }

            string contract;
            using (JsonDocument body = JsonDocument.Parse(stringValue))
            {
                contract = body.RootElement.GetProperty("name").GetString();
            }

            var impType = typeof(Command).Assembly.FindTypeByName(contract + "Command");

            if (impType == null)
            {
                throw new CommandNotFoundException(contract);
            }

            return (Command)JsonSerializer.Deserialize(stringValue, impType, SerializerSettings);
        }

        public override void SetValue(IDbDataParameter parameter, Command value)
        {
            parameter.Value = value == null ? null : JsonSerializer.Serialize(value, SerializerSettings);
        }
    }
}
