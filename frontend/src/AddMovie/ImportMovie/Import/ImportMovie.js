import { reduce } from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Alert from 'Components/Alert';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import { kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import selectAll from 'Utilities/Table/selectAll';
import toggleSelected from 'Utilities/Table/toggleSelected';
import ImportMovieFooterConnector from './ImportMovieFooterConnector';
import ImportMovieTableConnector from './ImportMovieTableConnector';

class ImportMovie extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.scrollerRef = React.createRef();

    this.state = {
      allSelected: false,
      allUnselected: false,
      lastToggled: null,
      selectedState: {},
      contentBody: null
    };
  }

  //
  // Listeners

  getSelectedIds = () => {
    return reduce(
      this.state.selectedState,
      (result, value, id) => {
        if (value) {
          result.push(id);
        }

        return result;
      },
      []
    );
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
      selectedState
    } = this.state;

    return (
      <PageContent title={translate('ImportMovies')}>
        <PageContentBody ref={this.scrollerRef} >
          {
            rootFoldersFetching ? <LoadingIndicator /> : null
          }

          {
            !rootFoldersFetching && !!rootFoldersError ?
              <Alert kind={kinds.DANGER}>
                {translate('UnableToLoadRootFolders')}
              </Alert> :
              null
          }

          {
            !rootFoldersError &&
            !rootFoldersFetching &&
            rootFoldersPopulated &&
            !unmappedFolders.length ?
              <Alert kind={kinds.INFO}>
                {translate('AllMoviesInPathHaveBeenImported', [path])}
              </Alert> :
              null
          }

          {
            !rootFoldersError &&
            !rootFoldersFetching &&
            rootFoldersPopulated &&
            !!unmappedFolders.length &&
            this.scrollerRef.current ?
              <ImportMovieTableConnector
                rootFolderId={rootFolderId}
                unmappedFolders={unmappedFolders}
                allSelected={allSelected}
                allUnselected={allUnselected}
                selectedState={selectedState}
                scroller={this.scrollerRef.current}
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
