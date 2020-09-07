import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { saveInteractiveImportItem, updateInteractiveImportItem } from 'Store/Actions/interactiveImportActions';
import createAllAuthorSelector from 'Store/Selectors/createAllAuthorsSelector';
import SelectAuthorModalContent from './SelectAuthorModalContent';

function createMapStateToProps() {
  return createSelector(
    createAllAuthorSelector(),
    (items) => {
      return {
        items: [...items].sort((a, b) => {
          if (a.sortName < b.sortName) {
            return -1;
          }

          if (a.sortName > b.sortName) {
            return 1;
          }

          return 0;
        })
      };
    }
  );
}

const mapDispatchToProps = {
  updateInteractiveImportItem,
  saveInteractiveImportItem
};

class SelectAuthorModalContentConnector extends Component {

  //
  // Listeners

  onAuthorSelect = (authorId) => {
    const author = _.find(this.props.items, { id: authorId });

    const ids = this.props.ids;

    ids.forEach((id) => {
      this.props.updateInteractiveImportItem({
        id,
        author,
        book: undefined,
        bookReleaseId: undefined,
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
      <SelectAuthorModalContent
        {...this.props}
        onAuthorSelect={this.onAuthorSelect}
      />
    );
  }
}

SelectAuthorModalContentConnector.propTypes = {
  ids: PropTypes.arrayOf(PropTypes.number).isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  saveInteractiveImportItem: PropTypes.func.isRequired,
  updateInteractiveImportItem: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(SelectAuthorModalContentConnector);
