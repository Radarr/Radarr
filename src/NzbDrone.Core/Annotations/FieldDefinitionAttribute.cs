using System;
using System.Runtime.CompilerServices;

namespace NzbDrone.Core.Annotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class FieldDefinitionAttribute : Attribute
    {
        public FieldDefinitionAttribute(int order)
        {
            Order = order;
        }

        public int Order { get; private set; }
        public string Label { get; set; }
        public string Unit { get; set; }
        public string HelpText { get; set; }
        public string HelpLink { get; set; }
        public FieldType Type { get; set; }
        public bool Advanced { get; set; }
        public Type SelectOptions { get; set; }
        public string Section { get; set; }
        public HiddenType Hidden { get; set; }
        public string RequestAction { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class FieldOptionAttribute : Attribute
    {
        public FieldOptionAttribute(string label = null, [CallerLineNumber] int order = 0)
        {
            Order = order;
            Label = label;
        }

        public int Order { get; private set; }
        public string Label { get; set; }
        public string Hint { get; set; }
    }

    public enum FieldType
    {
        Textbox,
        Number,
        Password,
        Checkbox,
        Select,
        Path,
        FilePath,
        Tag,
        Action,
        Url,
        Captcha,
        OAuth,
        Device,
        TagSelect
    }

    public enum HiddenType
    {
        Visible,
        Hidden,
        HiddenIfNotSet
    }
}
