import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchLanguages } from 'Store/Actions/settingsActions';
import { updateMovieFiles } from 'Store/Actions/movieFileActions';
import SelectLanguageModalContent from './SelectLanguageModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.languages,
    (languages) => {
      const {
        isFetching,
        isPopulated,
        error,
        items
      } = languages;

      return {
        isFetching,
        isPopulated,
        error,
        items
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchFetchLanguages: fetchLanguages,
  dispatchupdateMovieFiles: updateMovieFiles
};

class SelectLanguageModalContentConnector extends Component {

  //
  // Lifecycle

  componentDidMount = () => {
    if (!this.props.isPopulated) {
      this.props.dispatchFetchLanguages();
    }
  }

  //
  // Listeners

  onLanguageSelect = ({ value }) => {
    const languageId = parseInt(value);

    const language = _.find(this.props.items,
      (item) => item.id === languageId);
    const languages = [language];
    const movieFileIds = this.props.ids;

    this.props.dispatchupdateMovieFiles({ movieFileIds, languages });

    this.props.onModalClose(true);
  }

  //
  // Render

  render() {
    return (
      <SelectLanguageModalContent
        {...this.props}
        onLanguageSelect={this.onLanguageSelect}
      />
    );
  }
}

SelectLanguageModalContentConnector.propTypes = {
  ids: PropTypes.arrayOf(PropTypes.number).isRequired,
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  dispatchFetchLanguages: PropTypes.func.isRequired,
  dispatchupdateMovieFiles: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(SelectLanguageModalContentConnector);
