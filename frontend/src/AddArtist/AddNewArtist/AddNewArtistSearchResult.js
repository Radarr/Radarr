import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { icons, kinds, sizes } from 'Helpers/Props';
import HeartRating from 'Components/HeartRating';
import Icon from 'Components/Icon';
import Label from 'Components/Label';
import Link from 'Components/Link/Link';
import ArtistPoster from 'Artist/ArtistPoster';
import AddNewArtistModal from './AddNewArtistModal';
import styles from './AddNewArtistSearchResult.css';

class AddNewArtistSearchResult extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isNewAddSeriesModalOpen: false
    };
  }

  componentDidUpdate(prevProps) {
    if (!prevProps.isExistingArtist && this.props.isExistingArtist) {
      this.onAddSerisModalClose();
    }
  }

  //
  // Listeners

  onPress = () => {
    this.setState({ isNewAddSeriesModalOpen: true });
  }

  onAddSerisModalClose = () => {
    this.setState({ isNewAddSeriesModalOpen: false });
  }

  //
  // Render

  render() {
    const {
      foreignArtistId,
      artistName,
      nameSlug,
      year,
      network,
      status,
      overview,
      seasonCount,
      ratings,
      images,
      isExistingArtist,
      isSmallScreen
    } = this.props;

    const linkProps = isExistingArtist ? { to: `/artist/${nameSlug}` } : { onPress: this.onPress };
    let seasons = '1 Season';

    if (seasonCount > 1) {
      seasons = `${seasonCount} Seasons`;
    }

    return (
      <Link
        className={styles.searchResult}
        {...linkProps}
      >
        {
          !isSmallScreen &&
            <ArtistPoster
              className={styles.poster}
              images={images}
              size={250}
            />
        }

        <div>
          <div className={styles.name}>
            {artistName}

            {
              !name.contains(year) && !!year &&
                <span className={styles.year}>({year})</span>
            }

            {
              isExistingArtist &&
                <Icon
                  className={styles.alreadyExistsIcon}
                  name={icons.CHECK_CIRCLE}
                  size={36}
                  title="Already in your library"
                />
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
              !!network &&
                <Label size={sizes.LARGE}>
                  {network}
                </Label>
            }

            {
              !!seasonCount &&
                <Label size={sizes.LARGE}>
                  {seasons}
                </Label>
            }

            {
              status === 'ended' &&
                <Label
                  kind={kinds.DANGER}
                  size={sizes.LARGE}
                >
                  Ended
                </Label>
            }
          </div>

          <div className={styles.overview}>
            {overview}
          </div>
        </div>

        <AddNewArtistModal
          isOpen={this.state.isNewAddSeriesModalOpen && !isExistingArtist}
          foreignArtistId={foreignArtistId}
          artistName={artistName}
          year={year}
          overview={overview}
          images={images}
          onModalClose={this.onAddSerisModalClose}
        />
      </Link>
    );
  }
}

AddNewArtistSearchResult.propTypes = {
  foreignArtistId: PropTypes.string.isRequired,
  artistName: PropTypes.string.isRequired,
  nameSlug: PropTypes.string.isRequired,
  year: PropTypes.number,
  network: PropTypes.string,
  status: PropTypes.string.isRequired,
  overview: PropTypes.string,
  seasonCount: PropTypes.number,
  ratings: PropTypes.object.isRequired,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  isExistingArtist: PropTypes.bool.isRequired,
  isSmallScreen: PropTypes.bool.isRequired
};

export default AddNewArtistSearchResult;
