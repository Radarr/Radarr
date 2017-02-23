namespace NzbDrone.Core.Tv
{
    public enum MovieStatusType
    {
        TBA = 0, //Nothing yet announced, only rumors, but still IMDb page
        Announced = 1, //AirDate is announced
        InCinemas = 2,
        Released = 3 //Has at least one PreDB release
    }
}
