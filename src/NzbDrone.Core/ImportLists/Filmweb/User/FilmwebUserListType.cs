using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.ImportLists.Filmweb.User
{
    public enum FilmwebUserListType
    {
        [FieldOption(Label = "Want to See")]
        WantToSee = 0,
        [FieldOption(Label = "Rated")]
        Rated = 1,
        [FieldOption(Label = "Favorites")]
        Favorites = 2
    }
}
