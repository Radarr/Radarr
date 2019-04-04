import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchInteractiveImportTrackFiles, clearInteractiveImportTrackFiles } from 'Store/Actions/interactiveImportActions';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import ConfirmImportModalContent from './ConfirmImportModalContent';

function createMapStateToProps() {
  return createSelector(
    createClientSideCollectionSelector('interactiveImport.trackFiles'),
    (trackFiles) => {
      return trackFiles;
    }
  );
}

const mapDispatchToProps = {
  fetchInteractiveImportTrackFiles,
  clearInteractiveImportTrackFiles
};

class ConfirmImportModalContentConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      albums
    } = this.props;

    this.props.fetchInteractiveImportTrackFiles({ albumId: albums.map((x) => x.id) });
  }

  componentWillUnmount() {
    this.props.clearInteractiveImportTrackFiles();
  }

  //
  // Render

  render() {
    return (
      <ConfirmImportModalContent
        {...this.props}
      />
    );
  }
}

ConfirmImportModalContentConnector.propTypes = {
  albums: PropTypes.arrayOf(PropTypes.object).isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  fetchInteractiveImportTrackFiles: PropTypes.func.isRequired,
  clearInteractiveImportTrackFiles: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(ConfirmImportModalContentConnector);
