import getNewArtist from 'Utilities/Artist/getNewArtist';

function getNewAlbum(album, payload) {
  const {
    searchForNewAlbum = false
  } = payload;

  getNewArtist(album.artist, payload);

  album.addOptions = {
    searchForNewAlbum
  };
  album.monitored = true;

  return album;
}

export default getNewAlbum;
