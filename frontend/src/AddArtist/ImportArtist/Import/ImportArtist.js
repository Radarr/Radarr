import PropTypes from 'prop-types';
import React, { Component } from 'react';
import getSelectedIds from 'Utilities/Table/getSelectedIds';
import selectAll from 'Utilities/Table/selectAll';
import toggleSelected from 'Utilities/Table/toggleSelected';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBodyConnector from 'Components/Page/PageContentBodyConnector';
import ImportArtistTableConnector from './ImportArtistTableConnector';
import ImportArtistFooterConnector from './ImportArtistFooterConnector';

class ImportArtist extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      allSelected: false,
      allUnselected: false,
      lastToggled: null,
      selectedState: {},
      scroller: null
    };
  }

  //
  // Control

  setScrollerRef = (ref) => {
    this.setState({ scroller: ref });
  }

  //
  // Listeners

  getSelectedIds = () => {
    return getSelectedIds(this.state.selectedState, { parseIds: false });
  }

  onSelectAllChange = ({ value }) => {
    // Only select non-dupes
    this.setState(selectAll(this.state.selectedState, value));
  }

  onSelectedChange = ({ id, value, shiftKey = false }) => {
    this.setState((state) => {
      return toggleSelected(state, this.props.items, id, value, shiftKey);
    });
  }

  onRemoveSelectedStateItem = (id) => {
    this.setState((state) => {
      const selectedState = Object.assign({}, state.selectedState);
      delete selectedState[id];

      return {
        ...state,
        selectedState
      };
    });
  }

  onInputChange = ({ name, value }) => {
    this.props.onInputChange(this.getSelectedIds(), name, value);
  }

  onImportPress = () => {
    this.props.onImportPress(this.getSelectedIds());
  }

  //
  // Render

  render() {
    const {
      rootFolderId,
      path,
      rootFoldersFetching,
      rootFoldersPopulated,
      rootFoldersError,
      unmappedFolders,
      showMetadataProfile
    } = this.props;

    const {
      allSelected,
      allUnselected,
      selectedState,
      scroller
    } = this.state;

    return (
      <PageContent title="Import Artist">
        <PageContentBodyConnector
          registerScroller={this.setScrollerRef}
          onScroll={this.onScroll}
        >
          {
            rootFoldersFetching && !rootFoldersPopulated &&
              <LoadingIndicator />
          }

          {
            !rootFoldersFetching && !!rootFoldersError &&
              <div>Unable to load root folders</div>
          }

          {
            !rootFoldersError && rootFoldersPopulated && !unmappedFolders.length &&
              <div>
                All artist in {path} have been imported
              </div>
          }

          {
            !rootFoldersError && rootFoldersPopulated && !!unmappedFolders.length && scroller &&
              <ImportArtistTableConnector
                rootFolderId={rootFolderId}
                unmappedFolders={unmappedFolders}
                allSelected={allSelected}
                allUnselected={allUnselected}
                selectedState={selectedState}
                scroller={scroller}
                showMetadataProfile={showMetadataProfile}
                onSelectAllChange={this.onSelectAllChange}
                onSelectedChange={this.onSelectedChange}
                onRemoveSelectedStateItem={this.onRemoveSelectedStateItem}
              />
          }
        </PageContentBodyConnector>

        {
          !rootFoldersError && rootFoldersPopulated && !!unmappedFolders.length &&
            <ImportArtistFooterConnector
              selectedIds={this.getSelectedIds()}
              showMetadataProfile={showMetadataProfile}
              onInputChange={this.onInputChange}
              onImportPress={this.onImportPress}
            />
        }
      </PageContent>
    );
  }
}

ImportArtist.propTypes = {
  rootFolderId: PropTypes.number.isRequired,
  path: PropTypes.string,
  rootFoldersFetching: PropTypes.bool.isRequired,
  rootFoldersPopulated: PropTypes.bool.isRequired,
  rootFoldersError: PropTypes.object,
  unmappedFolders: PropTypes.arrayOf(PropTypes.object),
  items: PropTypes.arrayOf(PropTypes.object),
  showMetadataProfile: PropTypes.bool.isRequired,
  onInputChange: PropTypes.func.isRequired,
  onImportPress: PropTypes.func.isRequired
};

ImportArtist.defaultProps = {
  unmappedFolders: []
};

export default ImportArtist;
