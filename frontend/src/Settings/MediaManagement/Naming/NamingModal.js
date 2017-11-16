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

    this.state = {
      case: 'title'
    };
  }

  //
  // Listeners

  onNamingCaseChange = (event) => {
    this.setState({ case: event.value });
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

    const namingOptions = [
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
      { token: '{Artist.Name}', example: 'Artist.Name' },
      { token: '{Artist_Name}', example: 'Artist_Name' },

      { token: '{Artist NameThe}', example: 'Artist Name, The' },

      { token: '{Artist CleanName}', example: 'Artist Name' },
      { token: '{Artist.CleanName}', example: 'Artist.Name' },
      { token: '{Artist_CleanName}', example: 'Artist_Name' }
    ];

    const albumTokens = [
      { token: '{Album Title}', example: 'Album Title' },
      { token: '{Album.Title}', example: 'Album.Title' },
      { token: '{Album_Name}', example: 'Album_Name' },

      { token: '{Album TitleThe}', example: 'Album Title, The' },

      { token: '{Album CleanTitle}', example: 'Album Title' },
      { token: '{Album.CleanTitle}', example: 'Album.Title' },
      { token: '{Album_CleanTitle}', example: 'Album_Title' }
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
      { token: '{Track.Title}', example: 'Track.Title' },
      { token: '{Track_Title}', example: 'Track_Title' },
      { token: '{Track CleanTitle}', example: 'Track Title' },
      { token: '{Track.CleanTitle}', example: 'Track.Title' },
      { token: '{Track_CleanTitle}', example: 'Track_Title' }
    ];

    const qualityTokens = [
      { token: '{Quality Full}', example: 'HDTV 720p Proper' },
      { token: '{Quality-Full}', example: 'HDTV-720p-Proper' },
      { token: '{Quality.Full}', example: 'HDTV.720p.Proper' },
      { token: '{Quality_Full}', example: 'HDTV_720p_Proper' },
      { token: '{Quality Title}', example: 'HDTV 720p' },
      { token: '{Quality-Title}', example: 'HDTV-720p' },
      { token: '{Quality.Title}', example: 'HDTV.720p' },
      { token: '{Quality_Title}', example: 'HDTV_720p' }
    ];

    const mediaInfoTokens = [
      { token: '{MediaInfo Simple}', example: 'x264 DTS' },
      { token: '{MediaInfo.Simple}', example: 'x264.DTS' },
      { token: '{MediaInfo_Simple}', example: 'x264_DTS' },
      { token: '{MediaInfo Full}', example: 'x264 DTS [EN+DE]' },
      { token: '{MediaInfo.Full}', example: 'x264.DTS.[EN+DE]' },
      { token: '{MediaInfo_Full}', example: 'x264_DTS_[EN+DE]' },
      { token: '{MediaInfo VideoCodec}', example: 'x264' },
      { token: '{MediaInfo AudioFormat}', example: 'DTS' },
      { token: '{MediaInfo AudioChannels}', example: '5.1' }
    ];

    const releaseGroupTokens = [
      { token: '{Release Group}', example: 'Rls Grp' },
      { token: '{Release.Group}', example: 'Rls.Grp' },
      { token: '{Release_Group}', example: 'Rls_Grp' }
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
                name="namingSelect"
                value={this.state.case}
                values={namingOptions}
                onChange={this.onNamingCaseChange}
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
                            tokenCase={this.state.case}
                            size={sizes.LARGE}
                            onInputChange={onInputChange}
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
                        tokenCase={this.state.case}
                        onInputChange={onInputChange}
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
                              tokenCase={this.state.case}
                              onInputChange={onInputChange}
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
                              tokenCase={this.state.case}
                              onInputChange={onInputChange}
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
                              tokenCase={this.state.case}
                              onInputChange={onInputChange}
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
                              tokenCase={this.state.case}
                              onInputChange={onInputChange}
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
                              tokenCase={this.state.case}
                              onInputChange={onInputChange}
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
                              tokenCase={this.state.case}
                              onInputChange={onInputChange}
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
                              tokenCase={this.state.case}
                              onInputChange={onInputChange}
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
                              tokenCase={this.state.case}
                              onInputChange={onInputChange}
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
                              tokenCase={this.state.case}
                              onInputChange={onInputChange}
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
                              tokenCase={this.state.case}
                              size={sizes.LARGE}
                              onInputChange={onInputChange}
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
