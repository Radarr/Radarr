import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createDeepEqualSelector from 'Store/Selectors/createDeepEqualSelector';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import ArtistIndexFooter from './ArtistIndexFooter';

function createUnoptimizedSelector() {
  return createSelector(
    createClientSideCollectionSelector('artist', 'artistIndex'),
    (artist) => {
      return artist.items.map((s) => {
        const {
          monitored,
          status,
          statistics
        } = s;

        return {
          monitored,
          status,
          statistics
        };
      });
    }
  );
}

function createArtistSelector() {
  return createDeepEqualSelector(
    createUnoptimizedSelector(),
    (artist) => artist
  );
}

function createMapStateToProps() {
  return createSelector(
    createArtistSelector(),
    (artist) => {
      return {
        artist
      };
    }
  );
}

export default connect(createMapStateToProps)(ArtistIndexFooter);
