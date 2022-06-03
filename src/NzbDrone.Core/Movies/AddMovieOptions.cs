namespace NzbDrone.Core.Movies
{
    public class AddMovieOptions : MonitoringOptions
    {
        public bool SearchForMovie { get; set; }
        public AddMovieMethod AddMethod { get; set; }
    }

    public enum AddMovieMethod
    {
        Manual,
        List,
        Collection
    }
}
