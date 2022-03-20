import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { updateMovieFiles } from 'Store/Actions/movieFileActions';
import { fetchQualityProfileSchema } from 'Store/Actions/settingsActions';
import createMovieFileSelector from 'Store/Selectors/createMovieFileSelector';
import getQualities from 'Utilities/Quality/getQualities';
import FileEditModalContent from './FileEditModalContent';

function createMapStateToProps() {
  return createSelector(
    createMovieFileSelector(),
    (state) => state.settings.qualityProfiles,
    (state) => state.settings.languages,
    (movieFile, qualityProfiles, languages) => {

      const filterItems = ['Any', 'Original'];
      const filteredLanguages = languages.items.filter((lang) => !filterItems.includes(lang.name));

      const quality = movieFile.quality;

      return {
        isFetching: qualityProfiles.isSchemaFetching || languages.isFetching,
        isPopulated: qualityProfiles.isSchemaPopulated && languages.isPopulated,
        error: qualityProfiles.error || languages.error,
        qualityId: quality ? quality.quality.id : 0,
        real: quality ? quality.revision.real > 0 : false,
        proper: quality ? quality.revision.version > 1 : false,
        qualities: getQualities(qualityProfiles.schema.items),
        languageIds: movieFile.languages ? movieFile.languages.map((l) => l.id) : [],
        languages: filteredLanguages,
        indexerFlags: movieFile.indexerFlags,
        edition: movieFile.edition,
        releaseGroup: movieFile.releaseGroup,
        relativePath: movieFile.relativePath
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchFetchQualityProfileSchema: fetchQualityProfileSchema,
  dispatchUpdateMovieFiles: updateMovieFiles
};

class FileEditModalContentConnector extends Component {

  //
  // Lifecycle

  componentDidMount = () => {
    if (!this.props.isPopulated) {
      this.props.dispatchFetchQualityProfileSchema();
    }
  };

  //
  // Listeners

  onSaveInputs = ( payload ) => {
    const {
      qualityId,
      real,
      proper,
      languageIds,
      edition,
      releaseGroup,
      indexerFlags
    } = payload;

    const quality = this.props.qualities.find((item) => item.id === qualityId);

    const languages = [];

    languageIds.forEach((languageId) => {
      const language = this.props.languages.find((item) => item.id === parseInt(languageId));

      if (language !== undefined) {
        languages.push(language);
      }
    });

    const revision = {
      version: proper ? 2 : 1,
      real: real ? 1 : 0
    };

    const movieFileIds = [this.props.movieFileId];

    this.props.dispatchUpdateMovieFiles({
      movieFileIds,
      languages,
      indexerFlags,
      edition,
      releaseGroup,
      quality: {
        quality,
        revision
      }
    });

    this.props.onModalClose(true);
  };

  //
  // Render

  render() {
    return (
      <FileEditModalContent
        {...this.props}
        onSaveInputs={this.onSaveInputs}
      />
    );
  }
}

FileEditModalContentConnector.propTypes = {
  movieFileId: PropTypes.number.isRequired,
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  qualities: PropTypes.arrayOf(PropTypes.object).isRequired,
  languages: PropTypes.arrayOf(PropTypes.object).isRequired,
  languageIds: PropTypes.arrayOf(PropTypes.number).isRequired,
  indexerFlags: PropTypes.number.isRequired,
  qualityId: PropTypes.number.isRequired,
  real: PropTypes.bool.isRequired,
  edition: PropTypes.string.isRequired,
  releaseGroup: PropTypes.string.isRequired,
  relativePath: PropTypes.string.isRequired,
  proper: PropTypes.bool.isRequired,
  dispatchFetchQualityProfileSchema: PropTypes.func.isRequired,
  dispatchUpdateMovieFiles: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(FileEditModalContentConnector);
