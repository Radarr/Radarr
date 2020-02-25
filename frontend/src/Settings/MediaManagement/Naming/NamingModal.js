import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { sizes } from 'Helpers/Props';
import FieldSet from 'Components/FieldSet';
import Button from 'Components/Link/Button';
import SelectInput from 'Components/Form/SelectInput';
import TextInput from 'Components/Form/TextInput';
import Modal from 'Components/Modal/Modal';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';
import NamingOption from './NamingOption';
import styles from './NamingModal.css';

class NamingModal extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this._selectionStart = null;
    this._selectionEnd = null;

    this.state = {
      separator: ' ',
      case: 'title'
    };
  }

  //
  // Listeners

  onTokenSeparatorChange = (event) => {
    this.setState({ separator: event.value });
  }

  onTokenCaseChange = (event) => {
    this.setState({ case: event.value });
  }

  onInputSelectionChange = (selectionStart, selectionEnd) => {
    this._selectionStart = selectionStart;
    this._selectionEnd = selectionEnd;
  }

  onOptionPress = ({ isFullFilename, tokenValue }) => {
    const {
      name,
      value,
      onInputChange
    } = this.props;

    const selectionStart = this._selectionStart;
    const selectionEnd = this._selectionEnd;

    if (isFullFilename) {
      onInputChange({ name, value: tokenValue });
    } else if (selectionStart == null) {
      onInputChange({
        name,
        value: `${value}${tokenValue}`
      });
    } else {
      const start = value.substring(0, selectionStart);
      const end = value.substring(selectionEnd);
      const newValue = `${start}${tokenValue}${end}`;

      onInputChange({ name, value: newValue });
      this._selectionStart = newValue.length - 1;
      this._selectionEnd = newValue.length - 1;
    }
  }

  //
  // Render

  render() {
    const {
      name,
      value,
      isOpen,
      advancedSettings,
      additional,
      onInputChange,
      onModalClose
    } = this.props;

    const {
      separator: tokenSeparator,
      case: tokenCase
    } = this.state;

    const separatorOptions = [
      { key: ' ', value: 'Space ( )' },
      { key: '.', value: 'Period (.)' },
      { key: '_', value: 'Underscore (_)' },
      { key: '-', value: 'Dash (-)' }
    ];

    const caseOptions = [
      { key: 'title', value: 'Default Case' },
      { key: 'lower', value: 'Lower Case' },
      { key: 'upper', value: 'Upper Case' }
    ];

    const fileNameTokens = [
      {
        token: '{Movie Title} - {Quality Full}',
        example: 'Movie Title (2010) - HDTV-720p Proper'
      }
    ];

    const movieTokens = [
      { token: '{Movie Title}', example: 'Movie Title!' },
      { token: '{Movie CleanTitle}', example: 'Movie Title' },
      { token: '{Movie TitleThe}', example: 'Movie Title, The' }

    ];

    const movieIdTokens = [
      { token: '{ImdbId}', example: 'tt12345' },
      { token: '{TmdbId}', example: '123456' }
    ];

    const qualityTokens = [
      { token: '{Quality Full}', example: 'HDTV 720p Proper' },
      { token: '{Quality Title}', example: 'HDTV 720p' }
    ];

    const mediaInfoTokens = [
      { token: '{MediaInfo Simple}', example: 'x264 DTS' },
      { token: '{MediaInfo Full}', example: 'x264 DTS [EN+DE]' },
      { token: '{MediaInfo VideoCodec}', example: 'x264' },
      { token: '{MediaInfo AudioCodec}', example: 'DTS' },
      { token: '{MediaInfo AudioChannels}', example: '5.1' },
      { token: '{MediaInfo AudioLanguages}', example: '[EN+DE]' },
      { token: '{MediaInfo SubtitleLanguages}', example: '[DE]' },

      { token: '{MediaInfo VideoCodec}', example: 'x264' },
      { token: '{MediaInfo VideoBitDepth}', example: '10' },
      { token: '{MediaInfo VideoDynamicRange}', example: 'HDR' }
    ];

    const releaseGroupTokens = [
      { token: '{Release Group}', example: 'Rls Grp' }
    ];

    const editionTokens = [
      { token: '{Edition Tags}', example: 'IMAX' }
    ];

    const customFormatTokens = [
      { token: '{Custom Formats}', example: 'Surround Sound x264' }
    ];

    const originalTokens = [
      { token: '{Original Title}', example: 'Movie.Title.HDTV.x264-EVOLVE' },
      { token: '{Original Filename}', example: 'Movie.title.hdtv.x264-EVOLVE' }
    ];

    return (
      <Modal
        isOpen={isOpen}
        onModalClose={onModalClose}
      >
        <ModalContent onModalClose={onModalClose}>
          <ModalHeader>
            File Name Tokens
          </ModalHeader>

          <ModalBody>
            <div className={styles.namingSelectContainer}>
              <SelectInput
                className={styles.namingSelect}
                name="separator"
                value={tokenSeparator}
                values={separatorOptions}
                onChange={this.onTokenSeparatorChange}
              />

              <SelectInput
                className={styles.namingSelect}
                name="case"
                value={tokenCase}
                values={caseOptions}
                onChange={this.onTokenCaseChange}
              />
            </div>

            {
              !advancedSettings &&
                <FieldSet legend="File Names">
                  <div className={styles.groups}>
                    {
                      fileNameTokens.map(({ token, example }) => {
                        return (
                          <NamingOption
                            key={token}
                            name={name}
                            value={value}
                            token={token}
                            example={example}
                            isFullFilename={true}
                            tokenSeparator={tokenSeparator}
                            tokenCase={tokenCase}
                            size={sizes.LARGE}
                            onPress={this.onOptionPress}
                          />
                        );
                      }
                      )
                    }
                  </div>
                </FieldSet>
            }

            <FieldSet legend="Movie">
              <div className={styles.groups}>
                {
                  movieTokens.map(({ token, example }) => {
                    return (
                      <NamingOption
                        key={token}
                        name={name}
                        value={value}
                        token={token}
                        example={example}
                        tokenSeparator={tokenSeparator}
                        tokenCase={tokenCase}
                        onPress={this.onOptionPress}
                      />
                    );
                  }
                  )
                }
              </div>
            </FieldSet>

            <FieldSet legend="Movie ID">
              <div className={styles.groups}>
                {
                  movieIdTokens.map(({ token, example }) => {
                    return (
                      <NamingOption
                        key={token}
                        name={name}
                        value={value}
                        token={token}
                        example={example}
                        tokenSeparator={tokenSeparator}
                        tokenCase={tokenCase}
                        onPress={this.onOptionPress}
                      />
                    );
                  }
                  )
                }
              </div>
            </FieldSet>

            {
              additional &&
                <div>
                  <FieldSet legend="Quality">
                    <div className={styles.groups}>
                      {
                        qualityTokens.map(({ token, example }) => {
                          return (
                            <NamingOption
                              key={token}
                              name={name}
                              value={value}
                              token={token}
                              example={example}
                              tokenSeparator={tokenSeparator}
                              tokenCase={tokenCase}
                              onPress={this.onOptionPress}
                            />
                          );
                        }
                        )
                      }
                    </div>
                  </FieldSet>

                  <FieldSet legend="Media Info">
                    <div className={styles.groups}>
                      {
                        mediaInfoTokens.map(({ token, example }) => {
                          return (
                            <NamingOption
                              key={token}
                              name={name}
                              value={value}
                              token={token}
                              example={example}
                              tokenSeparator={tokenSeparator}
                              tokenCase={tokenCase}
                              onPress={this.onOptionPress}
                            />
                          );
                        }
                        )
                      }
                    </div>
                  </FieldSet>

                  <FieldSet legend="Release Group">
                    <div className={styles.groups}>
                      {
                        releaseGroupTokens.map(({ token, example }) => {
                          return (
                            <NamingOption
                              key={token}
                              name={name}
                              value={value}
                              token={token}
                              example={example}
                              tokenSeparator={tokenSeparator}
                              tokenCase={tokenCase}
                              onPress={this.onOptionPress}
                            />
                          );
                        }
                        )
                      }
                    </div>
                  </FieldSet>

                  <FieldSet legend="Edition">
                    <div className={styles.groups}>
                      {
                        editionTokens.map(({ token, example }) => {
                          return (
                            <NamingOption
                              key={token}
                              name={name}
                              value={value}
                              token={token}
                              example={example}
                              tokenSeparator={tokenSeparator}
                              tokenCase={tokenCase}
                              onPress={this.onOptionPress}
                            />
                          );
                        }
                        )
                      }
                    </div>
                  </FieldSet>

                  <FieldSet legend="Custom Formats">
                    <div className={styles.groups}>
                      {
                        customFormatTokens.map(({ token, example }) => {
                          return (
                            <NamingOption
                              key={token}
                              name={name}
                              value={value}
                              token={token}
                              example={example}
                              tokenSeparator={tokenSeparator}
                              tokenCase={tokenCase}
                              onPress={this.onOptionPress}
                            />
                          );
                        }
                        )
                      }
                    </div>
                  </FieldSet>

                  <FieldSet legend="Original">
                    <div className={styles.groups}>
                      {
                        originalTokens.map(({ token, example }) => {
                          return (
                            <NamingOption
                              key={token}
                              name={name}
                              value={value}
                              token={token}
                              example={example}
                              tokenSeparator={tokenSeparator}
                              tokenCase={tokenCase}
                              size={sizes.LARGE}
                              onPress={this.onOptionPress}
                            />
                          );
                        }
                        )
                      }
                    </div>
                  </FieldSet>
                </div>
            }
          </ModalBody>

          <ModalFooter>
            <TextInput
              name={name}
              value={value}
              onChange={onInputChange}
              onSelectionChange={this.onInputSelectionChange}
            />
            <Button onPress={onModalClose}>
              Close
            </Button>
          </ModalFooter>
        </ModalContent>
      </Modal>
    );
  }
}

NamingModal.propTypes = {
  name: PropTypes.string.isRequired,
  value: PropTypes.string.isRequired,
  isOpen: PropTypes.bool.isRequired,
  advancedSettings: PropTypes.bool.isRequired,
  additional: PropTypes.bool.isRequired,
  onInputChange: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

NamingModal.defaultProps = {
  additional: false
};

export default NamingModal;
