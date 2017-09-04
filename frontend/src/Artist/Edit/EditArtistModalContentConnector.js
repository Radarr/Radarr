import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import selectSettings from 'Store/Selectors/selectSettings';
import createArtistSelector from 'Store/Selectors/createArtistSelector';
import { setSeriesValue, saveArtist } from 'Store/Actions/seriesActions';
import EditArtistModalContent from './EditArtistModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.series,
    (state) => state.settings.languageProfiles,
    createArtistSelector(),
    (seriesState, languageProfiles, series) => {
      const {
        isSaving,
        saveError,
        pendingChanges
      } = seriesState;

      const seriesSettings = _.pick(series, [
        'monitored',
        'albumFolder',
        'qualityProfileId',
        'languageProfileId',
        // 'seriesType',
        'path',
        'tags'
      ]);

      const settings = selectSettings(seriesSettings, pendingChanges, saveError);

      return {
        artistName: series.artistName,
        isSaving,
        saveError,
        pendingChanges,
        item: settings.settings,
        showLanguageProfile: languageProfiles.items.length > 1,
        ...settings
      };
    }
  );
}

const mapDispatchToProps = {
  setSeriesValue,
  saveArtist
};

class EditArtistModalContentConnector extends Component {

  //
  // Lifecycle

  componentDidUpdate(prevProps, prevState) {
    if (prevProps.isSaving && !this.props.isSaving && !this.props.saveError) {
      this.props.onModalClose();
    }
  }

  //
  // Listeners

  onInputChange = ({ name, value }) => {
    this.props.setSeriesValue({ name, value });
  }

  onSavePress = () => {
    this.props.saveArtist({ id: this.props.artistId });
  }

  //
  // Render

  render() {
    return (
      <EditArtistModalContent
        {...this.props}
        onInputChange={this.onInputChange}
        onSavePress={this.onSavePress}
      />
    );
  }
}

EditArtistModalContentConnector.propTypes = {
  artistId: PropTypes.number,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  setSeriesValue: PropTypes.func.isRequired,
  saveArtist: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(EditArtistModalContentConnector);
