import { connect } from 'react-redux';
import { push } from 'react-router-redux';
import { createSelector } from 'reselect';
import jdu from 'jdu';
import createAllArtistSelector from 'Store/Selectors/createAllArtistSelector';
import createTagsSelector from 'Store/Selectors/createTagsSelector';
import ArtistSearchInput from './ArtistSearchInput';

function createCleanTagsSelector() {
  return createSelector(
    createTagsSelector(),
    (tags) => {
      return tags.map((tag) => {
        const {
          id,
          label
        } = tag;

        return {
          id,
          label,
          cleanLabel: jdu.replace(label).toLowerCase()
        };
      });
    }
  );
}

function createCleanArtistSelector() {
  return createSelector(
    createAllArtistSelector(),
    createCleanTagsSelector(),
    (allArtists, allTags) => {
      return allArtists.map((artist) => {
        const {
          artistName,
          sortName,
          images,
          foreignArtistId,
          // alternateTitles,
          tags = []
        } = artist;

        return {
          artistName,
          sortName,
          foreignArtistId,
          images,
          cleanName: jdu.replace(artistName).toLowerCase(),
          // alternateTitles: alternateTitles.map((alternateTitle) => {
          //   return {
          //     title: alternateTitle.title,
          //     cleanTitle: jdu.replace(alternateTitle.title).toLowerCase()
          //   };
          // }),
          tags: tags.map((id) => {
            return allTags.find((tag) => tag.id === id);
          })
        };
      }).sort((a, b) => {
        if (a.sortName < b.sortName) {
          return -1;
        }
        if (a.sortName > b.sortName) {
          return 1;
        }

        return 0;
      });
    }
  );
}

function createMapStateToProps() {
  return createSelector(
    createCleanArtistSelector(),
    (artist) => {
      return {
        artist
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onGoToArtist(foreignArtistId) {
      dispatch(push(`${window.Lidarr.urlBase}/artist/${foreignArtistId}`));
    },

    onGoToAddNewArtist(query) {
      dispatch(push(`${window.Lidarr.urlBase}/add/new?term=${encodeURIComponent(query)}`));
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(ArtistSearchInput);
