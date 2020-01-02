import moment from 'moment';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import TextTruncate from 'react-text-truncate';
import dimensions from 'Styles/Variables/dimensions';
import fonts from 'Styles/Variables/fonts';
import { icons, sizes } from 'Helpers/Props';
import HeartRating from 'Components/HeartRating';
import Icon from 'Components/Icon';
import Label from 'Components/Label';
import Link from 'Components/Link/Link';
import AlbumCover from 'Album/AlbumCover';
import AddNewAlbumModal from './AddNewAlbumModal';
import styles from './AddNewAlbumSearchResult.css';

const columnPadding = parseInt(dimensions.artistIndexColumnPadding);
const columnPaddingSmallScreen = parseInt(dimensions.artistIndexColumnPaddingSmallScreen);
const defaultFontSize = parseInt(fonts.defaultFontSize);
const lineHeight = parseFloat(fonts.lineHeight);

function calculateHeight(rowHeight, isSmallScreen) {
  let height = rowHeight - 70;

  if (isSmallScreen) {
    height -= columnPaddingSmallScreen;
  } else {
    height -= columnPadding;
  }

  return height;
}

class AddNewAlbumSearchResult extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isNewAddAlbumModalOpen: false
    };
  }

  componentDidUpdate(prevProps) {
    if (!prevProps.isExistingAlbum && this.props.isExistingAlbum) {
      this.onAddAlbumModalClose();
    }
  }

  //
  // Listeners

  onPress = () => {
    this.setState({ isNewAddAlbumModalOpen: true });
  }

  onAddAlbumModalClose = () => {
    this.setState({ isNewAddAlbumModalOpen: false });
  }

  onMBLinkPress = (event) => {
    event.stopPropagation();
  }

  //
  // Render

  render() {
    const {
      foreignAlbumId,
      title,
      releaseDate,
      disambiguation,
      albumType,
      secondaryTypes,
      overview,
      ratings,
      images,
      releases,
      artist,
      isExistingAlbum,
      isExistingArtist,
      isSmallScreen
    } = this.props;

    const {
      isNewAddAlbumModalOpen
    } = this.state;

    const linkProps = isExistingAlbum ? { to: `/album/${foreignAlbumId}` } : { onPress: this.onPress };

    const height = calculateHeight(230, isSmallScreen);

    return (
      <div className={styles.searchResult}>
        <Link
          className={styles.underlay}
          {...linkProps}
        />

        <div className={styles.overlay}>
          {
            !isSmallScreen &&
            <AlbumCover
              className={styles.poster}
              images={images}
              size={250}
            />
          }

          <div className={styles.content}>
            <div className={styles.name}>
              {title}

              {
                !!disambiguation &&
                <span className={styles.year}>({disambiguation})</span>
              }

              {
                isExistingAlbum ?
                  <Icon
                    className={styles.alreadyExistsIcon}
                    name={icons.CHECK_CIRCLE}
                    size={20}
                    title="Album already in your library"
                  /> :
                  null
              }

              <Link
                className={styles.mbLink}
                to={`https://musicbrainz.org/release-group/${foreignAlbumId}`}
                onPress={this.onMBLinkPress}
              >
                <Icon
                  className={styles.mbLinkIcon}
                  name={icons.EXTERNAL_LINK}
                  size={28}
                />
              </Link>

            </div>

            <div>
              <span className={styles.artistName}> By: {artist.artistName}</span>

              {
                isExistingArtist ?
                  <Icon
                    className={styles.alreadyExistsIcon}
                    name={icons.CHECK_CIRCLE}
                    size={15}
                    title="Artist already in your library"
                  /> :
                  null
              }
            </div>

            <div>
              <Label size={sizes.LARGE}>
                <HeartRating
                  rating={ratings.value}
                  iconSize={13}
                />
              </Label>

              {
                !!releaseDate &&
                  <Label size={sizes.LARGE}>
                    {moment(releaseDate).format('YYYY')}
                  </Label>
              }

              <Label size={sizes.LARGE}>
                {releases.length} release{releases.length > 0 ? 's' : null}
              </Label>

              {
                !!albumType &&
                  <Label size={sizes.LARGE}>
                    {albumType}
                  </Label>
              }

              {
                !!secondaryTypes &&
                  secondaryTypes.map((item, i) => {
                    return (
                      <Label
                        size={sizes.LARGE}
                        key={i}
                      >
                        {item}
                      </Label>
                    );
                  })
              }

            </div>

            <div
              className={styles.overview}
              style={{
                maxHeight: `${height}px`
              }}
            >
              <TextTruncate
                truncateText="…"
                line={Math.floor(height / (defaultFontSize * lineHeight))}
                text={overview}
              />
            </div>
          </div>
        </div>

        <AddNewAlbumModal
          isOpen={isNewAddAlbumModalOpen && !isExistingAlbum}
          isExistingArtist={isExistingArtist}
          foreignAlbumId={foreignAlbumId}
          albumTitle={title}
          disambiguation={disambiguation}
          artistName={artist.artistName}
          overview={overview}
          images={images}
          onModalClose={this.onAddAlbumModalClose}
        />
      </div>
    );
  }
}

AddNewAlbumSearchResult.propTypes = {
  foreignAlbumId: PropTypes.string.isRequired,
  title: PropTypes.string.isRequired,
  releaseDate: PropTypes.string.isRequired,
  disambiguation: PropTypes.string,
  albumType: PropTypes.string,
  secondaryTypes: PropTypes.arrayOf(PropTypes.string).isRequired,
  overview: PropTypes.string,
  ratings: PropTypes.object.isRequired,
  artist: PropTypes.object,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  releases: PropTypes.arrayOf(PropTypes.object).isRequired,
  isExistingAlbum: PropTypes.bool.isRequired,
  isExistingArtist: PropTypes.bool.isRequired,
  isSmallScreen: PropTypes.bool.isRequired
};

export default AddNewAlbumSearchResult;
