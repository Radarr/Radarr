/* eslint max-params: 0 */
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import * as commandNames from 'Commands/commandNames';
import withScrollPosition from 'Components/withScrollPosition';
import { setAuthorFilter, setAuthorSort, setAuthorTableOption, setAuthorView } from 'Store/Actions/authorIndexActions';
import { executeCommand } from 'Store/Actions/commandActions';
import scrollPositions from 'Store/scrollPositions';
import createAuthorClientSideCollectionItemsSelector from 'Store/Selectors/createAuthorClientSideCollectionItemsSelector';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import AuthorIndex from './AuthorIndex';

function createMapStateToProps() {
  return createSelector(
    createAuthorClientSideCollectionItemsSelector('authorIndex'),
    createCommandExecutingSelector(commandNames.REFRESH_AUTHOR),
    createCommandExecutingSelector(commandNames.RSS_SYNC),
    createDimensionsSelector(),
    (
      author,
      isRefreshingAuthor,
      isRssSyncExecuting,
      dimensionsState
    ) => {
      return {
        ...author,
        isRefreshingAuthor,
        isRssSyncExecuting,
        isSmallScreen: dimensionsState.isSmallScreen
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onTableOptionChange(payload) {
      dispatch(setAuthorTableOption(payload));
    },

    onSortSelect(sortKey) {
      dispatch(setAuthorSort({ sortKey }));
    },

    onFilterSelect(selectedFilterKey) {
      dispatch(setAuthorFilter({ selectedFilterKey }));
    },

    dispatchSetAuthorView(view) {
      dispatch(setAuthorView({ view }));
    },

    onRefreshAuthorPress() {
      dispatch(executeCommand({
        name: commandNames.REFRESH_AUTHOR
      }));
    },

    onRssSyncPress() {
      dispatch(executeCommand({
        name: commandNames.RSS_SYNC
      }));
    }
  };
}

class AuthorIndexConnector extends Component {

  //
  // Listeners

  onViewSelect = (view) => {
    this.props.dispatchSetAuthorView(view);
  }

  onScroll = ({ scrollTop }) => {
    scrollPositions.authorIndex = scrollTop;
  }

  //
  // Render

  render() {
    return (
      <AuthorIndex
        {...this.props}
        onViewSelect={this.onViewSelect}
        onScroll={this.onScroll}
      />
    );
  }
}

AuthorIndexConnector.propTypes = {
  isSmallScreen: PropTypes.bool.isRequired,
  view: PropTypes.string.isRequired,
  dispatchSetAuthorView: PropTypes.func.isRequired
};

export default withScrollPosition(
  connect(createMapStateToProps, createMapDispatchToProps)(AuthorIndexConnector),
  'authorIndex'
);
