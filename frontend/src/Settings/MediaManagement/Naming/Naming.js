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
        name: 'standardBookFormat',
        album: true,
        track: true,
        additional: true
      }
    });
  }

  onAuthorFolderNamingModalOpenClick = () => {
    this.setState({
      isNamingModalOpen: true,
      namingModalOptions: {
        name: 'authorFolderFormat'
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

    const renameBooks = hasSettings && settings.renameBooks.value;

    const standardBookFormatHelpTexts = [];
    const standardBookFormatErrors = [];
    const authorFolderFormatHelpTexts = [];
    const authorFolderFormatErrors = [];

    if (examplesPopulated) {
      if (examples.singleTrackExample) {
        standardBookFormatHelpTexts.push(`Single Track: ${examples.singleTrackExample}`);
      } else {
        standardBookFormatErrors.push({ message: 'Single Track: Invalid Format' });
      }

      if (examples.artistFolderExample) {
        authorFolderFormatHelpTexts.push(`Example: ${examples.artistFolderExample}`);
      } else {
        authorFolderFormatErrors.push({ message: 'Invalid Format' });
      }
    }

    return (
      <FieldSet legend="Track Naming">
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
                <FormLabel>Rename Tracks</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="renameBooks"
                  helpText="Readarr will use the existing file name if renaming is disabled"
                  onChange={onInputChange}
                  {...settings.renameBooks}
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
                renameBooks &&
                  <div>
                    <FormGroup size={sizes.LARGE}>
                      <FormLabel>Standard Book Format</FormLabel>

                      <FormInputGroup
                        inputClassName={styles.namingInput}
                        type={inputTypes.TEXT}
                        name="standardBookFormat"
                        buttons={<FormInputButton onPress={this.onStandardNamingModalOpenClick}>?</FormInputButton>}
                        onChange={onInputChange}
                        {...settings.standardBookFormat}
                        helpTexts={standardBookFormatHelpTexts}
                        errors={[...standardBookFormatErrors, ...settings.standardBookFormat.errors]}
                      />
                    </FormGroup>
                  </div>
              }

              <FormGroup
                advancedSettings={advancedSettings}
                isAdvanced={true}
              >
                <FormLabel>Artist Folder Format</FormLabel>

                <FormInputGroup
                  inputClassName={styles.namingInput}
                  type={inputTypes.TEXT}
                  name="authorFolderFormat"
                  buttons={<FormInputButton onPress={this.onAuthorFolderNamingModalOpenClick}>?</FormInputButton>}
                  onChange={onInputChange}
                  {...settings.authorFolderFormat}
                  helpTexts={['Used when adding a new artist or moving an author via the author editor', ...authorFolderFormatHelpTexts]}
                  errors={[...authorFolderFormatErrors, ...settings.authorFolderFormat.errors]}
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
