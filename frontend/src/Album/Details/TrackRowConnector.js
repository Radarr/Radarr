/* eslint max-params: 0 */
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createTrackFileSelector from 'Store/Selectors/createTrackFileSelector';
import createCommandsSelector from 'Store/Selectors/createCommandsSelector';
import TrackRow from './TrackRow';

function createMapStateToProps() {
  return createSelector(
    (state, { id }) => id,
    (state, { mediumNumber }) => mediumNumber,
    createTrackFileSelector(),
    createCommandsSelector(),
    (id, mediumNumber, trackFile, commands) => {
      return {
        trackFilePath: trackFile ? trackFile.path : null,
        trackFileRelativePath: trackFile ? trackFile.relativePath : null
      };
    }
  );
}
export default connect(createMapStateToProps)(TrackRow);
