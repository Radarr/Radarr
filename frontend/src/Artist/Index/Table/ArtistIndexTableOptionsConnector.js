import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import ArtistIndexTableOptions from './ArtistIndexTableOptions';

function createMapStateToProps() {
  return createSelector(
    (state) => state.artistIndex.tableOptions,
    (tableOptions) => {
      return tableOptions;
    }
  );
}

export default connect(createMapStateToProps)(ArtistIndexTableOptions);
