import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createRouteMatchShape from 'Helpers/Props/Shapes/createRouteMatchShape';
import { setAddMovieDefault } from 'Store/Actions/addMovieActions';
import { clearImportMovie, importMovie, setImportMovieValue } from 'Store/Actions/importMovieActions';
import { fetchRootFolders } from 'Store/Actions/rootFolderActions';
import ImportMovie from './ImportMovie';

function createMapStateToProps() {
  return createSelector(
    (state, { match }) => match,
    (state) => state.rootFolders,
    (state) => state.addMovie,
    (state) => state.importMovie,
    (state) => state.settings.qualityProfiles,
    (
      match,
      rootFolders,
      addMovie,
      importMovieState,
      qualityProfiles
    ) => {
      const {
        isFetching: rootFoldersFetching,
        isPopulated: rootFoldersPopulated,
        error: rootFoldersError,
        items
      } = rootFolders;

      const rootFolderId = parseInt(match.params.rootFolderId);

      const result = {
        rootFolderId,
        rootFoldersFetching,
        rootFoldersPopulated,
        rootFoldersError,
        qualityProfiles: qualityProfiles.items,
        defaultQualityProfileId: addMovie.defaults.qualityProfileId
      };

      if (items.length) {
        const rootFolder = _.find(items, { id: rootFolderId });

        return {
          ...result,
          ...rootFolder,
          items: importMovieState.items
        };
      }

      return result;
    }
  );
}

const mapDispatchToProps = {
  dispatchSetImportMovieValue: setImportMovieValue,
  dispatchImportMovie: importMovie,
  dispatchClearImportMovie: clearImportMovie,
  dispatchFetchRootFolders: fetchRootFolders,
  dispatchSetAddMovieDefault: setAddMovieDefault
};

class ImportMovieConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      rootFolderId,
      qualityProfiles,
      defaultQualityProfileId,
      dispatchFetchRootFolders,
      dispatchSetAddMovieDefault
    } = this.props;

    dispatchFetchRootFolders({ id: rootFolderId, timeout: false });

    let setDefaults = false;
    const setDefaultPayload = {};

    if (
      !defaultQualityProfileId ||
      !qualityProfiles.some((p) => p.id === defaultQualityProfileId)
    ) {
      setDefaults = true;
      setDefaultPayload.qualityProfileId = qualityProfiles[0].id;
    }

    if (setDefaults) {
      dispatchSetAddMovieDefault(setDefaultPayload);
    }
  }

  componentWillUnmount() {
    this.props.dispatchClearImportMovie();
  }

  //
  // Listeners

  onInputChange = (ids, name, value) => {
    this.props.dispatchSetAddMovieDefault({ [name]: value });

    ids.forEach((id) => {
      this.props.dispatchSetImportMovieValue({
        id,
        [name]: value
      });
    });
  };

  onImportPress = (ids) => {
    this.props.dispatchImportMovie({ ids });
  };

  //
  // Render

  render() {
    return (
      <ImportMovie
        {...this.props}
        onInputChange={this.onInputChange}
        onImportPress={this.onImportPress}
      />
    );
  }
}

const routeMatchShape = createRouteMatchShape({
  rootFolderId: PropTypes.string.isRequired
});

ImportMovieConnector.propTypes = {
  match: routeMatchShape.isRequired,
  rootFolderId: PropTypes.number.isRequired,
  rootFoldersFetching: PropTypes.bool.isRequired,
  rootFoldersPopulated: PropTypes.bool.isRequired,
  qualityProfiles: PropTypes.arrayOf(PropTypes.object).isRequired,
  defaultQualityProfileId: PropTypes.number.isRequired,
  dispatchSetImportMovieValue: PropTypes.func.isRequired,
  dispatchImportMovie: PropTypes.func.isRequired,
  dispatchClearImportMovie: PropTypes.func.isRequired,
  dispatchFetchRootFolders: PropTypes.func.isRequired,
  dispatchSetAddMovieDefault: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(ImportMovieConnector);
