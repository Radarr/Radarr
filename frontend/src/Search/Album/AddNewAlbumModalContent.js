import PropTypes from 'prop-types';
import React, { Component } from 'react';
import TextTruncate from 'react-text-truncate';
import { kinds } from 'Helpers/Props';
import SpinnerButton from 'Components/Link/SpinnerButton';
import CheckInput from 'Components/Form/CheckInput';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';
import AlbumCover from 'Album/AlbumCover';
import AddArtistOptionsForm from '../Common/AddArtistOptionsForm.js';
import styles from './AddNewAlbumModalContent.css';

class AddNewAlbumModalContent extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      searchForNewAlbum: false
    };
  }

  //
  // Listeners

  onSearchForNewAlbumChange = ({ value }) => {
    this.setState({ searchForNewAlbum: value });
  }

  onAddAlbumPress = () => {
    this.props.onAddAlbumPress(this.state.searchForNewAlbum);
  }

  //
  // Render

  render() {
    const {
      albumTitle,
      artistName,
      disambiguation,
      overview,
      images,
      isAdding,
      isExistingArtist,
      isSmallScreen,
      onModalClose,
      ...otherProps
    } = this.props;

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          Add new Album
        </ModalHeader>

        <ModalBody>
          <div className={styles.container}>
            {
              isSmallScreen ?
                null:
                <div className={styles.poster}>
                  <AlbumCover
                    className={styles.poster}
                    images={images}
                    size={250}
                  />
                </div>
            }

            <div className={styles.info}>
              <div className={styles.name}>
                {albumTitle}
              </div>

              {
                !!disambiguation &&
                  <span className={styles.disambiguation}>({disambiguation})</span>
              }

              <div>
                <span className={styles.artistName}> By: {artistName}</span>
              </div>

              {
                overview ?
                  <div className={styles.overview}>
                    <TextTruncate
                      truncateText="…"
                      line={8}
                      text={overview}
                    />
                  </div> :
                  null
              }

              {
                !isExistingArtist &&
                  <AddArtistOptionsForm
                    artistName={artistName}
                    includeNoneMetadataProfile={true}
                    {...otherProps}
                  />
              }
            </div>
          </div>
        </ModalBody>

        <ModalFooter className={styles.modalFooter}>
          <label className={styles.searchForNewAlbumLabelContainer}>
            <span className={styles.searchForNewAlbumLabel}>
              Start search for new book
            </span>

            <CheckInput
              containerClassName={styles.searchForNewAlbumContainer}
              className={styles.searchForNewAlbumInput}
              name="searchForNewAlbum"
              value={this.state.searchForNewAlbum}
              onChange={this.onSearchForNewAlbumChange}
            />
          </label>

          <SpinnerButton
            className={styles.addButton}
            kind={kinds.SUCCESS}
            isSpinning={isAdding}
            onPress={this.onAddAlbumPress}
          >
            Add {albumTitle}
          </SpinnerButton>
        </ModalFooter>
      </ModalContent>
    );
  }
}

AddNewAlbumModalContent.propTypes = {
  albumTitle: PropTypes.string.isRequired,
  artistName: PropTypes.string.isRequired,
  disambiguation: PropTypes.string.isRequired,
  overview: PropTypes.string,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  isAdding: PropTypes.bool.isRequired,
  addError: PropTypes.object,
  isExistingArtist: PropTypes.bool.isRequired,
  isSmallScreen: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired,
  onAddAlbumPress: PropTypes.func.isRequired
};

export default AddNewAlbumModalContent;
