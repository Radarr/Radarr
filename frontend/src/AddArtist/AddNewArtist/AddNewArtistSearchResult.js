import PropTypes from 'prop-types';
import React, { Component } from 'react';
import TextTruncate from 'react-text-truncate';
import dimensions from 'Styles/Variables/dimensions';
import fonts from 'Styles/Variables/fonts';
import { icons, kinds, sizes } from 'Helpers/Props';
import HeartRating from 'Components/HeartRating';
import Icon from 'Components/Icon';
import Label from 'Components/Label';
import Link from 'Components/Link/Link';
import ArtistPoster from 'Artist/ArtistPoster';
import AddNewArtistModal from './AddNewArtistModal';
import styles from './AddNewArtistSearchResult.css';

const columnPadding = parseInt(dimensions.artistIndexColumnPadding);
const columnPaddingSmallScreen = parseInt(dimensions.artistIndexColumnPaddingSmallScreen);
const defaultFontSize = parseInt(fonts.defaultFontSize);
const lineHeight = parseFloat(fonts.lineHeight);

function calculateHeight(rowHeight, isSmallScreen) {
  let height = rowHeight - 45;

  if (isSmallScreen) {
    height -= columnPaddingSmallScreen;
  } else {
    height -= columnPadding;
  }

  return height;
}

class AddNewArtistSearchResult extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isNewAddArtistModalOpen: false
    };
  }

  componentDidUpdate(prevProps) {
    if (!prevProps.isExistingArtist && this.props.isExistingArtist) {
      this.onAddArtistModalClose();
    }
  }

  //
  // Listeners

  onPress = () => {
    this.setState({ isNewAddArtistModalOpen: true });
  }

  onAddArtistModalClose = () => {
    this.setState({ isNewAddArtistModalOpen: false });
  }

  onMBLinkPress = (event) => {
    event.stopPropagation();
  }

  //
  // Render

  render() {
    const {
      foreignArtistId,
      artistName,
      year,
      disambiguation,
      artistType,
      status,
      overview,
      ratings,
      images,
      isExistingArtist,
      isSmallScreen
    } = this.props;

    const {
      isNewAddArtistModalOpen
    } = this.state;

    const linkProps = isExistingArtist ? { to: `/artist/${foreignArtistId}` } : { onPress: this.onPress };

    const endedString = artistType === 'Person' ? 'Deceased' : 'Ended';

    const height = calculateHeight(230, isSmallScreen);

    return (
      <div>
        <Link
          className={styles.searchResult}
          {...linkProps}
        >
          {
            isSmallScreen ?
              null :
              <ArtistPoster
                className={styles.poster}
                images={images}
                size={250}
                overflow={true}
              />
          }

          <div className={styles.content}>
            <div className={styles.name}>
              {artistName}

              {
                !name.contains(year) && year ?
                  <span className={styles.year}>
                    ({year})
                  </span> :
                  null
              }

              {
                !!disambiguation &&
                <span className={styles.year}>({disambiguation})</span>
              }

              {
                isExistingArtist ?
                  <Icon
                    className={styles.alreadyExistsIcon}
                    name={icons.CHECK_CIRCLE}
                    size={36}
                    title="Already in your library"
                  /> :
                  null
              }

              <Link
                className={styles.mbLink}
                to={`https://musicbrainz.org/artist/${foreignArtistId}`}
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
              <Label size={sizes.LARGE}>
                <HeartRating
                  rating={ratings.value}
                  iconSize={13}
                />
              </Label>

              {
                artistType ?
                  <Label size={sizes.LARGE}>
                    {artistType}
                  </Label> :
                  null
              }

              {
                status === 'ended' ?
                  <Label
                    kind={kinds.DANGER}
                    size={sizes.LARGE}
                  >
                    {endedString}
                  </Label> :
                  null
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
        </Link>

        <AddNewArtistModal
          isOpen={isNewAddArtistModalOpen && !isExistingArtist}
          foreignArtistId={foreignArtistId}
          artistName={artistName}
          year={year}
          overview={overview}
          images={images}
          onModalClose={this.onAddArtistModalClose}
        />
      </div>
    );
  }
}

AddNewArtistSearchResult.propTypes = {
  foreignArtistId: PropTypes.string.isRequired,
  artistName: PropTypes.string.isRequired,
  year: PropTypes.number,
  disambiguation: PropTypes.string,
  artistType: PropTypes.string,
  status: PropTypes.string.isRequired,
  overview: PropTypes.string,
  ratings: PropTypes.object.isRequired,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  isExistingArtist: PropTypes.bool.isRequired,
  isSmallScreen: PropTypes.bool.isRequired
};

export default AddNewArtistSearchResult;
