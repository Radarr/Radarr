import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { isCommandExecuting } from 'Utilities/Command';
import createArtistSelector from 'Store/Selectors/createArtistSelector';
import createCommandsSelector from 'Store/Selectors/createCommandsSelector';
import { executeCommand } from 'Store/Actions/commandActions';
import * as commandNames from 'Commands/commandNames';
import AlbumSearchCell from './AlbumSearchCell';

function createMapStateToProps() {
  return createSelector(
    (state, { albumId }) => albumId,
    createArtistSelector(),
    createCommandsSelector(),
    (albumId, artist, commands) => {
      const isSearching = commands.some((command) => {
        const albumSearch = command.name === commandNames.ALBUM_SEARCH;

        if (!albumSearch) {
          return false;
        }

        return (
          isCommandExecuting(command) &&
          command.body.albumIds.indexOf(albumId) > -1
        );
      });

      return {
        artistMonitored: artist.monitored,
        artistType: artist.artistType,
        isSearching
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onSearchPress(name, path) {
      dispatch(executeCommand({
        name: commandNames.ALBUM_SEARCH,
        albumIds: [props.albumId]
      }));
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(AlbumSearchCell);
