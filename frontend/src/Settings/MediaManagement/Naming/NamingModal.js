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
      album,
      track,
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
        token: '{Artist Name} - {Album Title} - {track:00} - {Track Title} {Quality Full}',
        example: 'Artist Name - Album Title - 01 - Track Title MP3-320 Proper'
      },
      {
        token: '{Artist.Name}.{Album.Title}.{track:00}.{TrackClean.Title}.{Quality.Full}',
        example: 'Artist.Name.Album.Title.01.Track.Title.MP3-320'
      }
    ];

    const artistTokens = [
      { token: '{Artist Name}', example: 'Artist Name' },

      { token: '{Artist NameThe}', example: 'Artist Name, The' },

      { token: '{Artist CleanName}', example: 'Artist Name' }
    ];

    const albumTokens = [
      { token: '{Album Title}', example: 'Album Title' },

      { token: '{Album TitleThe}', example: 'Album Title, The' },

      { token: '{Album CleanTitle}', example: 'Album Title' },

      { token: '{Album Type}', example: 'Album Type' },

      { token: '{Album Disambiguation}', example: 'Disambiguation' }
    ];

    const mediumTokens = [
      { token: '{medium:0}', example: '1' },
      { token: '{medium:00}', example: '01' }
    ];

    const mediumFormatTokens = [
      { token: '{Medium Format}', example: 'CD' }
    ];

    const trackTokens = [
      { token: '{track:0}', example: '1' },
      { token: '{track:00}', example: '01' }
    ];

    const releaseDateTokens = [
      { token: '{Release Year}', example: '2016' }
    ];

    const trackTitleTokens = [
      { token: '{Track Title}', example: 'Track Title' },
      { token: '{Track CleanTitle}', example: 'Track Title' }
    ];

    const qualityTokens = [
      { token: '{Quality Full}', example: 'FLAC Proper' },
      { token: '{Quality Title}', example: 'FLAC' }
    ];

    const mediaInfoTokens = [
      { token: '{MediaInfo Simple}', example: 'x264 DTS' },
      { token: '{MediaInfo Full}', example: 'x264 DTS [EN+DE]' },
      { token: '{MediaInfo VideoCodec}', example: 'x264' },
      { token: '{MediaInfo AudioFormat}', example: 'DTS' },
      { token: '{MediaInfo AudioChannels}', example: '5.1' }
    ];

    const releaseGroupTokens = [
      { token: '{Release Group}', example: 'Rls Grp' }
    ];

    const originalTokens = [
      { token: '{Original Title}', example: 'Artist.Name.S01E01.HDTV.x264-EVOLVE' },
      { token: '{Original Filename}', example: 'artist.name.s01e01.hdtv.x264-EVOLVE' }
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

            <FieldSet legend="Artist">
              <div className={styles.groups}>
                {
                  artistTokens.map(({ token, example }) => {
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
              album &&
                <div>
                  <FieldSet legend="Album">
                    <div className={styles.groups}>
                      {
                        albumTokens.map(({ token, example }) => {
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

                  <FieldSet legend="Release Date">
                    <div className={styles.groups}>
                      {
                        releaseDateTokens.map(({ token, example }) => {
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
                </div>
            }

            {
              track &&
                <div>
                  <FieldSet legend="Medium">
                    <div className={styles.groups}>
                      {
                        mediumTokens.map(({ token, example }) => {
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

                  <FieldSet legend="Medium Format">
                    <div className={styles.groups}>
                      {
                        mediumFormatTokens.map(({ token, example }) => {
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

                  <FieldSet legend="Track">
                    <div className={styles.groups}>
                      {
                        trackTokens.map(({ token, example }) => {
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

                </div>
            }

            {
              additional &&
                <div>
                  <FieldSet legend="Track Title">
                    <div className={styles.groups}>
                      {
                        trackTitleTokens.map(({ token, example }) => {
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
  album: PropTypes.bool.isRequired,
  track: PropTypes.bool.isRequired,
  additional: PropTypes.bool.isRequired,
  onInputChange: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

NamingModal.defaultProps = {
  album: false,
  track: false,
  additional: false
};

export default NamingModal;
