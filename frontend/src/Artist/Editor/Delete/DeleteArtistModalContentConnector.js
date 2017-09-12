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
    (artistIds, allSeries) => {
      const selectedSeries = _.intersectionWith(allSeries, artistIds, (s, id) => {
        return s.id === id;
      });

      const sortedArtist = _.orderBy(selectedSeries, 'sortName');
      const series = _.map(sortedArtist, (s) => {
        return {
          artistName: s.artistName,
          path: s.path
        };
      });

      return {
        series
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
