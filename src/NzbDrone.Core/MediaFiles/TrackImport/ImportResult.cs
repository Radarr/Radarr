using NzbDrone.Common.EnsureThat;
using NzbDrone.Core.Parser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.MediaFiles.TrackImport
{
    public class ImportResult
    {
        public ImportDecision<LocalTrack> ImportDecision { get; private set; }
        public List<string> Errors { get; private set; }

        public ImportResultType Result
        {
            get
            {
                if (Errors.Any())
                {
                    if (ImportDecision.Approved)
                    {
                        return ImportResultType.Skipped;
                    }

                    return ImportResultType.Rejected;
                }

                return ImportResultType.Imported;
            }
        }

        public ImportResult(ImportDecision<LocalTrack> importDecision, params string[] errors)
        {
            Ensure.That(importDecision, () => importDecision).IsNotNull();

            ImportDecision = importDecision;
            Errors = errors.ToList();
        }
    }
}
