using System;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using Readarr.Http.REST;

namespace Readarr.Api.V1.BookFiles
{
    public class BookFileResource : RestResource
    {
        public int AuthorId { get; set; }
        public int BookId { get; set; }
        public string Path { get; set; }
        public long Size { get; set; }
        public DateTime DateAdded { get; set; }
        public QualityModel Quality { get; set; }
        public int QualityWeight { get; set; }
        public MediaInfoResource MediaInfo { get; set; }

        public bool QualityCutoffNotMet { get; set; }
        public ParsedTrackInfo AudioTags { get; set; }
    }

    public static class BookFileResourceMapper
    {
        private static int QualityWeight(QualityModel quality)
        {
            if (quality == null)
            {
                return 0;
            }

            int qualityWeight = Quality.DefaultQualityDefinitions.Single(q => q.Quality == quality.Quality).Weight;
            qualityWeight += quality.Revision.Real * 10;
            qualityWeight += quality.Revision.Version;
            return qualityWeight;
        }

        public static BookFileResource ToResource(this BookFile model)
        {
            if (model == null)
            {
                return null;
            }

            return new BookFileResource
            {
                Id = model.Id,
                BookId = model.EditionId,
                Path = model.Path,
                Size = model.Size,
                DateAdded = model.DateAdded,
                Quality = model.Quality,
                QualityWeight = QualityWeight(model.Quality),
                MediaInfo = model.MediaInfo.ToResource()
            };
        }

        public static BookFileResource ToResource(this BookFile model, NzbDrone.Core.Books.Author author, IUpgradableSpecification upgradableSpecification)
        {
            if (model == null)
            {
                return null;
            }

            return new BookFileResource
            {
                Id = model.Id,

                AuthorId = author.Id,
                BookId = model.EditionId,
                Path = model.Path,
                Size = model.Size,
                DateAdded = model.DateAdded,
                Quality = model.Quality,
                QualityWeight = QualityWeight(model.Quality),
                MediaInfo = model.MediaInfo.ToResource(),
                QualityCutoffNotMet = upgradableSpecification.QualityCutoffNotMet(author.QualityProfile.Value, model.Quality)
            };
        }
    }
}
