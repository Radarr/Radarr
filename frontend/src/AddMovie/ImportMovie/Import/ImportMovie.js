import PropTypes from 'prop-types';
import React, { Component } from 'react';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import translate from 'Utilities/String/translate';
import getSelectedIds from 'Utilities/Table/getSelectedIds';
import selectAll from 'Utilities/Table/selectAll';
import toggleSelected from 'Utilities/Table/toggleSelected';
import ImportMovieFooterConnector from './ImportMovieFooterConnector';
import ImportMovieTableConnector from './ImportMovieTableConnector';

class ImportMovie extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      allSelected: false,
      allUnselected: false,
      lastToggled: null,
      selectedState: {},
      contentBody: null
    };
  }

  //
  // Control

  setScrollerRef = (ref) => {
    this.setState({ scroller: ref });
  };

  //
  // Listeners

  getSelectedIds = () => {
    return getSelectedIds(this.state.selectedState, { parseIds: false });
  };

  onSelectAllChange = ({ value }) => {
    // Only select non-dupes
    this.setState(selectAll(this.state.selectedState, value));
  };

  onSelectedChange = ({ id, value, shiftKey = false }) => {
    this.setState((state) => {
      return toggleSelected(state, this.props.items, id, value, shiftKey);
    });
  };

  onRemoveSelectedStateItem = (id) => {
    this.setState((state) => {
      const selectedState = Object.assign({}, state.selectedState);
      delete selectedState[id];

      return {
        ...state,
        selectedState
      };
    });
  };

  onInputChange = ({ name, value }) => {
    this.props.onInputChange(this.getSelectedIds(), name, value);
  };

  onImportPress = () => {
    this.props.onImportPress(this.getSelectedIds());
  };

  //
  // Render

  render() {
    const {
      rootFolderId,
      path,
      rootFoldersFetching,
      rootFoldersError,
      rootFoldersPopulated,
      unmappedFolders
    } = this.props;

    const {
      allSelected,
      allUnselected,
      selectedState,
      scroller
    } = this.state;

    return (
      <PageContent title={translate('ImportMovies')}>
        <PageContentBody
          registerScroller={this.setScrollerRef}
          onScroll={this.onScroll}
        >
          {
            rootFoldersFetching ? <LoadingIndicator /> : null
          }

          {
            !rootFoldersFetching && !!rootFoldersError ?
              <div>
                {translate('UnableToLoadRootFolders')}
              </div> :
              null
          }

          {
            !rootFoldersError &&
            !rootFoldersFetching &&
            rootFoldersPopulated &&
            !unmappedFolders.length ?
              <div>
                {translate('AllMoviesInPathHaveBeenImported', [path])}
              </div> :
              null
          }

          {
            !rootFoldersError &&
            !rootFoldersFetching &&
            rootFoldersPopulated &&
            !!unmappedFolders.length &&
            scroller ?
              <ImportMovieTableConnector
                rootFolderId={rootFolderId}
                unmappedFolders={unmappedFolders}
                allSelected={allSelected}
                allUnselected={allUnselected}
                selectedState={selectedState}
                scroller={scroller}
                onSelectAllChange={this.onSelectAllChange}
                onSelectedChange={this.onSelectedChange}
                onRemoveSelectedStateItem={this.onRemoveSelectedStateItem}
              /> :
              null
          }
        </PageContentBody>

        {
          !rootFoldersError &&
          !rootFoldersFetching &&
          !!unmappedFolders.length ?
            <ImportMovieFooterConnector
              selectedIds={this.getSelectedIds()}
              onInputChange={this.onInputChange}
              onImportPress={this.onImportPress}
            /> :
            null
        }
      </PageContent>
    );
  }
}

ImportMovie.propTypes = {
  rootFolderId: PropTypes.number.isRequired,
  path: PropTypes.string,
  rootFoldersFetching: PropTypes.bool.isRequired,
  rootFoldersPopulated: PropTypes.bool.isRequired,
  rootFoldersError: PropTypes.object,
  unmappedFolders: PropTypes.arrayOf(PropTypes.object),
  items: PropTypes.arrayOf(PropTypes.object),
  onInputChange: PropTypes.func.isRequired,
  onImportPress: PropTypes.func.isRequired
};

ImportMovie.defaultProps = {
  unmappedFolders: []
};

export default ImportMovie;
