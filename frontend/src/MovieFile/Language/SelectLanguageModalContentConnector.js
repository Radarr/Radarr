import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import SelectLanguageModalContent from 'InteractiveImport/Language/SelectLanguageModalContent';
import { updateMovieFiles } from 'Store/Actions/movieFileActions';
import { fetchLanguages } from 'Store/Actions/settingsActions';

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

      const filterItems = ['Any', 'Original'];
      const filteredLanguages = items.filter((lang) => !filterItems.includes(lang.name));

      return {
        isFetching,
        isPopulated,
        error,
        items: filteredLanguages
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
  };

  //
  // Listeners

  onLanguageSelect = ({ languageIds }) => {
    const languages = [];

    languageIds.forEach((languageId) => {
      const language = _.find(this.props.items,
        (item) => item.id === parseInt(languageId));

      if (language !== undefined) {
        languages.push(language);
      }
    });

    this.props.dispatchupdateMovieFiles({
      movieFileIds: this.props.ids,
      languages
    });

    this.props.onModalClose(true);
  };

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
