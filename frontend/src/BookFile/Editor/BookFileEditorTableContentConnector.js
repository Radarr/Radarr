/* eslint max-params: 0 */
import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { deleteBookFiles, updateBookFiles } from 'Store/Actions/bookFileActions';
import { fetchQualityProfileSchema } from 'Store/Actions/settingsActions';
import createAuthorSelector from 'Store/Selectors/createAuthorSelector';
import getQualities from 'Utilities/Quality/getQualities';
import BookFileEditorTableContent from './BookFileEditorTableContent';

function createSchemaSelector() {
  return createSelector(
    (state) => state.settings.qualityProfiles,
    (qualityProfiles) => {
      const qualities = getQualities(qualityProfiles.schema.items);

      let error = null;

      if (qualityProfiles.schemaError) {
        error = 'Unable to load qualities';
      }

      return {
        isFetching: qualityProfiles.isSchemaFetching,
        isPopulated: qualityProfiles.isSchemaPopulated,
        error,
        qualities
      };
    }
  );
}

function createMapStateToProps() {
  return createSelector(
    (state, { bookId }) => bookId,
    (state) => state.bookFiles,
    createSchemaSelector(),
    createAuthorSelector(),
    (
      bookId,
      bookFiles,
      schema,
      author
    ) => {
      return {
        ...schema,
        items: bookFiles.items,
        authorType: author.authorType,
        isDeleting: bookFiles.isDeleting,
        isSaving: bookFiles.isSaving
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    dispatchFetchQualityProfileSchema(name, path) {
      dispatch(fetchQualityProfileSchema());
    },

    dispatchUpdateBookFiles(updateProps) {
      dispatch(updateBookFiles(updateProps));
    },

    onDeletePress(bookFileIds) {
      dispatch(deleteBookFiles({ bookFileIds }));
    }
  };
}

class BookFileEditorTableContentConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.dispatchFetchQualityProfileSchema();
  }

  //
  // Listeners

  onQualityChange = (bookFileIds, qualityId) => {
    const quality = {
      quality: _.find(this.props.qualities, { id: qualityId }),
      revision: {
        version: 1,
        real: 0
      }
    };

    this.props.dispatchUpdateBookFiles({ bookFileIds, quality });
  }

  //
  // Render

  render() {
    const {
      dispatchFetchQualityProfileSchema,
      dispatchUpdateBookFiles,
      ...otherProps
    } = this.props;

    return (
      <BookFileEditorTableContent
        {...otherProps}
        onQualityChange={this.onQualityChange}
      />
    );
  }
}

BookFileEditorTableContentConnector.propTypes = {
  authorId: PropTypes.number.isRequired,
  bookId: PropTypes.number,
  qualities: PropTypes.arrayOf(PropTypes.object).isRequired,
  dispatchFetchQualityProfileSchema: PropTypes.func.isRequired,
  dispatchUpdateBookFiles: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, createMapDispatchToProps)(BookFileEditorTableContentConnector);
