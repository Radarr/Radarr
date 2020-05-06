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
import ArtistPoster from 'Artist/ArtistPoster';
import AddArtistOptionsForm from '../Common/AddArtistOptionsForm.js';
import styles from './AddNewArtistModalContent.css';

class AddNewArtistModalContent extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      searchForMissingAlbums: false
    };
  }

  //
  // Listeners

  onSearchForMissingAlbumsChange = ({ value }) => {
    this.setState({ searchForMissingAlbums: value });
  }

  onAddArtistPress = () => {
    this.props.onAddArtistPress(this.state.searchForMissingAlbums);
  }

  //
  // Render

  render() {
    const {
      artistName,
      disambiguation,
      overview,
      images,
      isAdding,
      isSmallScreen,
      onModalClose,
      ...otherProps
    } = this.props;

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          Add new Artist
        </ModalHeader>

        <ModalBody>
          <div className={styles.container}>
            {
              isSmallScreen ?
                null:
                <div className={styles.poster}>
                  <ArtistPoster
                    className={styles.poster}
                    images={images}
                    size={250}
                  />
                </div>
            }

            <div className={styles.info}>
              <div className={styles.name}>
                {artistName}
              </div>

              {
                !!disambiguation &&
                  <span className={styles.disambiguation}>({disambiguation})</span>
              }

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

              <AddArtistOptionsForm
                includeNoneMetadataProfile={false}
                {...otherProps}
              />

            </div>
          </div>
        </ModalBody>

        <ModalFooter className={styles.modalFooter}>
          <label className={styles.searchForMissingAlbumsLabelContainer}>
            <span className={styles.searchForMissingAlbumsLabel}>
              Start search for missing books
            </span>

            <CheckInput
              containerClassName={styles.searchForMissingAlbumsContainer}
              className={styles.searchForMissingAlbumsInput}
              name="searchForMissingAlbums"
              value={this.state.searchForMissingAlbums}
              onChange={this.onSearchForMissingAlbumsChange}
            />
          </label>

          <SpinnerButton
            className={styles.addButton}
            kind={kinds.SUCCESS}
            isSpinning={isAdding}
            onPress={this.onAddArtistPress}
          >
            Add {artistName}
          </SpinnerButton>
        </ModalFooter>
      </ModalContent>
    );
  }
}

AddNewArtistModalContent.propTypes = {
  artistName: PropTypes.string.isRequired,
  disambiguation: PropTypes.string.isRequired,
  overview: PropTypes.string,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  isAdding: PropTypes.bool.isRequired,
  addError: PropTypes.object,
  isSmallScreen: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired,
  onAddArtistPress: PropTypes.func.isRequired
};

export default AddNewArtistModalContent;
