import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import {
  fetchInteractiveImportItems,
  setInteractiveImportSort,
  clearInteractiveImport,
  setInteractiveImportMode,
  updateInteractiveImportItem,
  saveInteractiveImportItem
} from 'Store/Actions/interactiveImportActions';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import { executeCommand } from 'Store/Actions/commandActions';
import * as commandNames from 'Commands/commandNames';
import InteractiveImportModalContent from './InteractiveImportModalContent';

function createMapStateToProps() {
  return createSelector(
    createClientSideCollectionSelector('interactiveImport'),
    (interactiveImport) => {
      return interactiveImport;
    }
  );
}

const mapDispatchToProps = {
  fetchInteractiveImportItems,
  setInteractiveImportSort,
  setInteractiveImportMode,
  clearInteractiveImport,
  updateInteractiveImportItem,
  saveInteractiveImportItem,
  executeCommand
};

class InteractiveImportModalContentConnector extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      interactiveImportErrorMessage: null,
      filterExistingFiles: props.filterExistingFiles,
      replaceExistingFiles: props.replaceExistingFiles
    };
  }

  componentDidMount() {
    const {
      authorId,
      downloadId,
      folder
    } = this.props;

    const {
      filterExistingFiles,
      replaceExistingFiles
    } = this.state;

    this.props.fetchInteractiveImportItems({
      authorId,
      downloadId,
      folder,
      filterExistingFiles,
      replaceExistingFiles
    });
  }

  componentDidUpdate(prevProps, prevState) {
    const {
      filterExistingFiles,
      replaceExistingFiles
    } = this.state;

    if (prevState.filterExistingFiles !== filterExistingFiles ||
        prevState.replaceExistingFiles !== replaceExistingFiles) {
      const {
        authorId,
        downloadId,
        folder
      } = this.props;

      this.props.fetchInteractiveImportItems({
        authorId,
        downloadId,
        folder,
        filterExistingFiles,
        replaceExistingFiles
      });
    }
  }

  componentWillUnmount() {
    this.props.clearInteractiveImport();
  }

  //
  // Listeners

  onSortPress = (sortKey, sortDirection) => {
    this.props.setInteractiveImportSort({ sortKey, sortDirection });
  }

  onFilterExistingFilesChange = (filterExistingFiles) => {
    this.setState({ filterExistingFiles });
  }

  onReplaceExistingFilesChange = (replaceExistingFiles) => {
    this.setState({ replaceExistingFiles });
  }

  onImportModeChange = (importMode) => {
    this.props.setInteractiveImportMode({ importMode });
  }

  onImportSelectedPress = (selected, importMode) => {
    const files = [];

    _.forEach(this.props.items, (item) => {
      const isSelected = selected.indexOf(item.id) > -1;

      if (isSelected) {
        const {
          author,
          book,
          quality,
          disableReleaseSwitching
        } = item;

        if (!author) {
          this.setState({ interactiveImportErrorMessage: 'Author must be chosen for each selected file' });
          return false;
        }

        if (!book) {
          this.setState({ interactiveImportErrorMessage: 'Book must be chosen for each selected file' });
          return false;
        }

        if (!quality) {
          this.setState({ interactiveImportErrorMessage: 'Quality must be chosen for each selected file' });
          return false;
        }

        files.push({
          path: item.path,
          authorId: author.id,
          bookId: book.id,
          quality,
          downloadId: this.props.downloadId,
          disableReleaseSwitching
        });
      }
    });

    if (!files.length) {
      return;
    }

    this.props.executeCommand({
      name: commandNames.INTERACTIVE_IMPORT,
      files,
      importMode,
      replaceExistingFiles: this.state.replaceExistingFiles
    });

    this.props.onModalClose();
  }

  //
  // Render

  render() {
    const {
      interactiveImportErrorMessage,
      filterExistingFiles,
      replaceExistingFiles
    } = this.state;

    return (
      <InteractiveImportModalContent
        {...this.props}
        interactiveImportErrorMessage={interactiveImportErrorMessage}
        filterExistingFiles={filterExistingFiles}
        replaceExistingFiles={replaceExistingFiles}
        onSortPress={this.onSortPress}
        onFilterExistingFilesChange={this.onFilterExistingFilesChange}
        onReplaceExistingFilesChange={this.onReplaceExistingFilesChange}
        onImportModeChange={this.onImportModeChange}
        onImportSelectedPress={this.onImportSelectedPress}
      />
    );
  }
}

InteractiveImportModalContentConnector.propTypes = {
  authorId: PropTypes.number,
  downloadId: PropTypes.string,
  folder: PropTypes.string,
  filterExistingFiles: PropTypes.bool.isRequired,
  replaceExistingFiles: PropTypes.bool.isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  fetchInteractiveImportItems: PropTypes.func.isRequired,
  setInteractiveImportSort: PropTypes.func.isRequired,
  clearInteractiveImport: PropTypes.func.isRequired,
  setInteractiveImportMode: PropTypes.func.isRequired,
  updateInteractiveImportItem: PropTypes.func.isRequired,
  executeCommand: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

InteractiveImportModalContentConnector.defaultProps = {
  authorId: 0,
  filterExistingFiles: true,
  replaceExistingFiles: false
};

export default connect(createMapStateToProps, mapDispatchToProps)(InteractiveImportModalContentConnector);
