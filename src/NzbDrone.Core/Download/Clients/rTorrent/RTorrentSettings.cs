﻿using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Download.Clients.RTorrent
{
    public class RTorrentSettingsValidator : AbstractValidator<RTorrentSettings>
    {
        public RTorrentSettingsValidator()
        {
            RuleFor(c => c.Host).ValidHost();
            RuleFor(c => c.Port).InclusiveBetween(1, 65535);
            RuleFor(c => c.MovieCategory).NotEmpty()
                                      .WithMessage("A category is recommended")
                                      .AsWarning();
        }
    }

    public class RTorrentSettings : IProviderConfig
    {
        private static readonly RTorrentSettingsValidator Validator = new RTorrentSettingsValidator();

        public RTorrentSettings()
        {
            Host = "localhost";
            Port = 8080;
            UrlBase = "RPC2";
            MovieCategory = "radarr";
        }

        [FieldDefinition(0, Label = "Host", Type = FieldType.Textbox)]
        public string Host { get; set; }

        [FieldDefinition(1, Label = "Port", Type = FieldType.Textbox)]
        public int Port { get; set; }

        [FieldDefinition(2, Label = "Url Path", Type = FieldType.Textbox, HelpText = "Path to the XMLRPC endpoint, see http(s)://[host]:[port]/[urlPath]. When using ruTorrent this usually is RPC2 or (path to ruTorrent)/plugins/rpc/rpc.php")]
        public string UrlBase { get; set; }

        [FieldDefinition(3, Label = "Use SSL", Type = FieldType.Checkbox)]
        public bool UseSsl { get; set; }

        [FieldDefinition(4, Label = "Username", Type = FieldType.Textbox)]
        public string Username { get; set; }

        [FieldDefinition(5, Label = "Password", Type = FieldType.Password)]
        public string Password { get; set; }

        [FieldDefinition(6, Label = "Category", Type = FieldType.Textbox, HelpText = "Adding a category specific to Radarr avoids conflicts with unrelated downloads, but it's optional.")]
        public string MovieCategory { get; set; }

        [FieldDefinition(7, Label = "Directory", Type = FieldType.Textbox, Advanced = true, HelpText = "Optional location to put downloads in, leave blank to use the default rTorrent location")]
        public string MovieDirectory { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
