import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import * as commandNames from 'Commands/commandNames';
import { executeCommand } from 'Store/Actions/commandActions';
import createAllAuthorSelector from 'Store/Selectors/createAllAuthorsSelector';
import RetagAuthorModalContent from './RetagAuthorModalContent';

function createMapStateToProps() {
  return createSelector(
    (state, { authorIds }) => authorIds,
    createAllAuthorSelector(),
    (authorIds, allAuthors) => {
      const author = _.intersectionWith(allAuthors, authorIds, (s, id) => {
        return s.id === id;
      });

      const sortedAuthor = _.orderBy(author, 'sortName');
      const authorNames = _.map(sortedAuthor, 'authorName');

      return {
        authorNames
      };
    }
  );
}

const mapDispatchToProps = {
  executeCommand
};

class RetagAuthorModalContentConnector extends Component {

  //
  // Listeners

  onRetagAuthorPress = () => {
    this.props.executeCommand({
      name: commandNames.RETAG_AUTHOR,
      authorIds: this.props.authorIds
    });

    this.props.onModalClose(true);
  }

  //
  // Render

  render(props) {
    return (
      <RetagAuthorModalContent
        {...this.props}
        onRetagAuthorPress={this.onRetagAuthorPress}
      />
    );
  }
}

RetagAuthorModalContentConnector.propTypes = {
  authorIds: PropTypes.arrayOf(PropTypes.number).isRequired,
  onModalClose: PropTypes.func.isRequired,
  executeCommand: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(RetagAuthorModalContentConnector);
