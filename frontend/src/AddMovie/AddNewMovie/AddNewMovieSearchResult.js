import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { icons, kinds, sizes } from 'Helpers/Props';
import HeartRating from 'Components/HeartRating';
import Icon from 'Components/Icon';
import Label from 'Components/Label';
import Link from 'Components/Link/Link';
import MoviePoster from 'Movie/MoviePoster';
import AddNewMovieModal from './AddNewMovieModal';
import styles from './AddNewMovieSearchResult.css';

class AddNewMovieSearchResult extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isNewAddMovieModalOpen: false
    };
  }

  componentDidUpdate(prevProps) {
    if (!prevProps.isExistingMovie && this.props.isExistingMovie) {
      this.onAddSerisModalClose();
    }
  }

  //
  // Listeners

  onPress = () => {
    this.setState({ isNewAddMovieModalOpen: true });
  }

  onAddSerisModalClose = () => {
    this.setState({ isNewAddMovieModalOpen: false });
  }

  //
  // Render

  render() {
    const {
      tmdbId,
      title,
      titleSlug,
      year,
      studio,
      status,
      overview,
      ratings,
      images,
      isExistingMovie,
      isSmallScreen
    } = this.props;
    const {
      isNewAddMovieModalOpen
    } = this.state;

    const linkProps = isExistingMovie ? { to: `/movie/${titleSlug}` } : { onPress: this.onPress };

    return (
      <div>
        <Link
          className={styles.searchResult}
          {...linkProps}
        >
          {
            isSmallScreen ?
              null :
              <MoviePoster
                className={styles.poster}
                images={images}
                size={250}
                overflow={true}
              />
          }

          <div>
            <div className={styles.title}>
              {title}

              {
                !title.contains(year) && !!year &&
                <span className={styles.year}>({year})</span>
              }

              {
                isExistingMovie &&
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
                !!studio &&
                <Label size={sizes.LARGE}>
                  {studio}
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
        </Link>

        <AddNewMovieModal
          isOpen={isNewAddMovieModalOpen && !isExistingMovie}
          tmdbId={tmdbId}
          title={title}
          year={year}
          overview={overview}
          images={images}
          onModalClose={this.onAddSerisModalClose}
        />
      </div>
    );
  }
}

AddNewMovieSearchResult.propTypes = {
  tmdbId: PropTypes.number.isRequired,
  title: PropTypes.string.isRequired,
  titleSlug: PropTypes.string.isRequired,
  year: PropTypes.number.isRequired,
  studio: PropTypes.string,
  status: PropTypes.string.isRequired,
  overview: PropTypes.string,
  ratings: PropTypes.object.isRequired,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  isExistingMovie: PropTypes.bool.isRequired,
  isSmallScreen: PropTypes.bool.isRequired
};

export default AddNewMovieSearchResult;
