namespace NzbDrone.Core.Tv
{
    public enum MovieStatusType
    {
        TBA = 0, //Nothing yet announced, only rumors, but still IMDb page
        Announced = 1, //AirDate is announced
        Released = 2 //Has at least one PreDB release
    }
}
