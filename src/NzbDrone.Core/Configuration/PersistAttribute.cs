using System;

namespace NzbDrone.Core.Configuration
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PersistAttribute : Attribute
    {
    }
}
