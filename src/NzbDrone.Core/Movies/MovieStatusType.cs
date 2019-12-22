namespace NzbDrone.Core.Movies
{
    public enum MovieStatusType
    {
        TBA = 0,       //Nothing yet announced, only rumors, but still IMDb page (this might not be used)
        Announced = 1, //Movie is announced but Cinema date is in the future or unknown
        InCinemas = 2, //Been in Cinemas for less than 3 months (since TMDB lacks complete information)
        Released = 3,  //Physical or Web Release or been in cinemas for > 3 months (since TMDB lacks complete information)
        PreDB = 4   //this is only used for MinimumAvailability. Movie items should never be in this state.
    }
}
