import _ from 'lodash';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createAllSeriesSelector from 'Store/Selectors/createAllSeriesSelector';
import { bulkDeleteArtist } from 'Store/Actions/seriesEditorActions';
import DeleteArtistModalContent from './DeleteArtistModalContent';

function createMapStateToProps() {
  return createSelector(
    (state, { artistIds }) => artistIds,
    createAllSeriesSelector(),
    (artistIds, allSeries) => {
      const selectedSeries = _.intersectionWith(allSeries, artistIds, (s, id) => {
        return s.id === id;
      });

      const sortedSeries = _.orderBy(selectedSeries, 'sortTitle');
      const series = _.map(sortedSeries, (s) => {
        return {
          title: s.title,
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
