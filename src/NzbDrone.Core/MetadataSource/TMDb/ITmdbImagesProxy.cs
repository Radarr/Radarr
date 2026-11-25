namespace NzbDrone.Core.MetadataSource.TMDb
{
    public interface ITmdbImagesProxy
    {
        NzbDrone.Core.MediaCover.MediaCover GetMovieLogo(int tmdbId);
    }
}
