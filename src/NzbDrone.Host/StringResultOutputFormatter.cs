using System;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace NzbDrone.Host
{
    // StringOutputFormatter writes string results only, but inherits a CanWriteType that accepts
    // every type, so ApiExplorer advertises text/plain for every action. Narrowing it keeps runtime
    // behaviour identical while the generated OpenAPI document lists text/plain only where it applies.
    public class StringResultOutputFormatter : StringOutputFormatter
    {
        protected override bool CanWriteType(Type type)
        {
            // Actions declared as object can still return a string at runtime.
            return type == null || type == typeof(object) || typeof(string).IsAssignableFrom(type);
        }
    }
}
