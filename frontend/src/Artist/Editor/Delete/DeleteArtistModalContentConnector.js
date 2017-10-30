import _ from 'lodash';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createAllArtistSelector from 'Store/Selectors/createAllArtistSelector';
import { bulkDeleteArtist } from 'Store/Actions/artistEditorActions';
import DeleteArtistModalContent from './DeleteArtistModalContent';

function createMapStateToProps() {
  return createSelector(
    (state, { artistIds }) => artistIds,
    createAllArtistSelector(),
    (artistIds, allArtists) => {
      const selectedArtist = _.intersectionWith(allArtists, artistIds, (s, id) => {
        return s.id === id;
      });

      const sortedArtist = _.orderBy(selectedArtist, 'sortName');
      const artist = _.map(sortedArtist, (s) => {
        return {
          artistName: s.artistName,
          path: s.path
        };
      });

      return {
        artist
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onDeleteSelectedPress(deleteFiles) {
      dispatch(bulkDeleteArtist({
        artistIds: props.artistIds,
        deleteFiles
      }));

      props.onModalClose();
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(DeleteArtistModalContent);
