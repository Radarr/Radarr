import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { authorHistoryMarkAsFailed, clearAuthorHistory, fetchAuthorHistory } from 'Store/Actions/authorHistoryActions';

function createMapStateToProps() {
  return createSelector(
    (state) => state.authorHistory,
    (authorHistory) => {
      return authorHistory;
    }
  );
}

const mapDispatchToProps = {
  fetchAuthorHistory,
  clearAuthorHistory,
  authorHistoryMarkAsFailed
};

class AuthorHistoryContentConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      authorId,
      bookId
    } = this.props;

    this.props.fetchAuthorHistory({
      authorId,
      bookId
    });
  }

  componentWillUnmount() {
    this.props.clearAuthorHistory();
  }

  //
  // Listeners

  onMarkAsFailedPress = (historyId) => {
    const {
      authorId,
      bookId
    } = this.props;

    this.props.authorHistoryMarkAsFailed({
      historyId,
      authorId,
      bookId
    });
  }

  //
  // Render

  render() {
    const {
      component: ViewComponent,
      ...otherProps
    } = this.props;

    return (
      <ViewComponent
        {...otherProps}
        onMarkAsFailedPress={this.onMarkAsFailedPress}
      />
    );
  }
}

AuthorHistoryContentConnector.propTypes = {
  component: PropTypes.elementType.isRequired,
  authorId: PropTypes.number.isRequired,
  bookId: PropTypes.number,
  fetchAuthorHistory: PropTypes.func.isRequired,
  clearAuthorHistory: PropTypes.func.isRequired,
  authorHistoryMarkAsFailed: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(AuthorHistoryContentConnector);
