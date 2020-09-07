import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import {
  saveInteractiveImportItem,
  updateInteractiveImportItem } from 'Store/Actions/interactiveImportActions';
import SelectEditionModalContent from './SelectEditionModalContent';

function createMapStateToProps() {
  return {};
}

const mapDispatchToProps = {
  updateInteractiveImportItem,
  saveInteractiveImportItem
};

class SelectEditionModalContentConnector extends Component {

  //
  // Listeners

  onEditionSelect = (bookId, editionId) => {
    const ids = this.props.importIdsByBook[bookId];

    ids.forEach((id) => {
      this.props.updateInteractiveImportItem({
        id,
        editionId,
        disableReleaseSwitching: true,
        tracks: [],
        rejections: []
      });
    });

    this.props.saveInteractiveImportItem({ id: ids });

    this.props.onModalClose(true);
  }

  //
  // Render

  render() {
    return (
      <SelectEditionModalContent
        {...this.props}
        onEditionSelect={this.onEditionSelect}
      />
    );
  }
}

SelectEditionModalContentConnector.propTypes = {
  importIdsByBook: PropTypes.object.isRequired,
  books: PropTypes.arrayOf(PropTypes.object).isRequired,
  updateInteractiveImportItem: PropTypes.func.isRequired,
  saveInteractiveImportItem: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(SelectEditionModalContentConnector);
