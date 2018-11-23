import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { inputTypes, sizes } from 'Helpers/Props';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import FormInputButton from 'Components/Form/FormInputButton';
import FieldSet from 'Components/FieldSet';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormLabel from 'Components/Form/FormLabel';
import FormInputGroup from 'Components/Form/FormInputGroup';
import NamingModal from './NamingModal';
import styles from './Naming.css';

class Naming extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isNamingModalOpen: false,
      namingModalOptions: null
    };
  }

  //
  // Listeners

  onStandardNamingModalOpenClick = () => {
    this.setState({
      isNamingModalOpen: true,
      namingModalOptions: {
        name: 'standardMovieFormat',
        additional: true
      }
    });
  }

  onMovieFolderNamingModalOpenClick = () => {
    this.setState({
      isNamingModalOpen: true,
      namingModalOptions: {
        name: 'movieFolderFormat'
      }
    });
  }

  onNamingModalClose = () => {
    this.setState({ isNamingModalOpen: false });
  }

  //
  // Render

  render() {
    const {
      advancedSettings,
      isFetching,
      error,
      settings,
      hasSettings,
      examples,
      examplesPopulated,
      onInputChange
    } = this.props;

    const {
      isNamingModalOpen,
      namingModalOptions
    } = this.state;

    const renameEpisodes = hasSettings && settings.renameEpisodes.value;

    const standardMovieFormatHelpTexts = [];
    const standardMovieFormatErrors = [];
    const movieFolderFormatHelpTexts = [];
    const movieFolderFormatErrors = [];

    if (examplesPopulated) {
      if (examples.movieExample) {
        standardMovieFormatHelpTexts.push(`Movie: ${examples.movieExample}`);
      } else {
        standardMovieFormatErrors.push({ message: 'Movie: Invalid Format' });
      }

      if (examples.movieFolderExample) {
        movieFolderFormatHelpTexts.push(`Example: ${examples.movieFolderExample}`);
      } else {
        movieFolderFormatErrors.push({ message: 'Invalid Format' });
      }
    }

    return (
      <FieldSet legend="Movie Naming">
        {
          isFetching &&
            <LoadingIndicator />
        }

        {
          !isFetching && error &&
            <div>Unable to load Naming settings</div>
        }

        {
          hasSettings && !isFetching && !error &&
            <Form>
              <FormGroup size={sizes.MEDIUM}>
                <FormLabel>Rename Movies</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="renameEpisodes"
                  helpText="Radarr will use the existing file name if renaming is disabled"
                  onChange={onInputChange}
                  {...settings.renameEpisodes}
                />
              </FormGroup>

              <FormGroup size={sizes.MEDIUM}>
                <FormLabel>Replace Illegal Characters</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="replaceIllegalCharacters"
                  helpText="Replace or Remove illegal characters"
                  onChange={onInputChange}
                  {...settings.replaceIllegalCharacters}
                />
              </FormGroup>

              {
                renameEpisodes &&
                  <div>
                    <FormGroup size={sizes.LARGE}>
                      <FormLabel>Standard Movie Format</FormLabel>

                      <FormInputGroup
                        inputClassName={styles.namingInput}
                        type={inputTypes.TEXT}
                        name="standardMovieFormat"
                        buttons={<FormInputButton onPress={this.onStandardNamingModalOpenClick}>?</FormInputButton>}
                        onChange={onInputChange}
                        {...settings.standardMovieFormat}
                        helpTexts={standardMovieFormatHelpTexts}
                        errors={[...standardMovieFormatErrors, ...settings.standardMovieFormat.errors]}
                      />
                    </FormGroup>
                  </div>
              }

              <FormGroup
                advancedSettings={advancedSettings}
                isAdvanced={true}
              >
                <FormLabel>Movie Folder Format</FormLabel>

                <FormInputGroup
                  inputClassName={styles.namingInput}
                  type={inputTypes.TEXT}
                  name="movieFolderFormat"
                  buttons={<FormInputButton onPress={this.onMovieFolderNamingModalOpenClick}>?</FormInputButton>}
                  onChange={onInputChange}
                  {...settings.movieFolderFormat}
                  helpTexts={['Used when adding a new movie or moving movies via the editor', ...movieFolderFormatHelpTexts]}
                  errors={[...movieFolderFormatErrors, ...settings.movieFolderFormat.errors]}
                />
              </FormGroup>

              {
                namingModalOptions &&
                  <NamingModal
                    isOpen={isNamingModalOpen}
                    advancedSettings={advancedSettings}
                    {...namingModalOptions}
                    value={settings[namingModalOptions.name].value}
                    onInputChange={onInputChange}
                    onModalClose={this.onNamingModalClose}
                  />
              }
            </Form>
        }
      </FieldSet>
    );
  }

}

Naming.propTypes = {
  advancedSettings: PropTypes.bool.isRequired,
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  settings: PropTypes.object.isRequired,
  hasSettings: PropTypes.bool.isRequired,
  examples: PropTypes.object.isRequired,
  examplesPopulated: PropTypes.bool.isRequired,
  onInputChange: PropTypes.func.isRequired
};

export default Naming;
