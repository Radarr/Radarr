import PropTypes from 'prop-types';
import React, { Component } from 'react';
import HeartRating from 'Components/HeartRating';
import Icon from 'Components/Icon';
import Label from 'Components/Label';
import Link from 'Components/Link/Link';
import Tooltip from 'Components/Tooltip/Tooltip';
import { icons, kinds, sizes, tooltipPositions } from 'Helpers/Props';
import MovieDetailsLinks from 'Movie/Details/MovieDetailsLinks';
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
      this.onAddMovieModalClose();
    }
  }

  //
  // Listeners

  onPress = () => {
    this.setState({ isNewAddMovieModalOpen: true });
  }

  onAddMovieModalClose = () => {
    this.setState({ isNewAddMovieModalOpen: false });
  }

  onExternalLinkPress = (event) => {
    event.stopPropagation();
  }

  //
  // Render

  render() {
    const {
      tmdbId,
      imdbId,
      youTubeTrailerId,
      title,
      titleSlug,
      year,
      studio,
      status,
      overview,
      ratings,
      folder,
      images,
      isExistingMovie,
      isExclusionMovie,
      isSmallScreen
    } = this.props;

    const {
      isNewAddMovieModalOpen
    } = this.state;

    const linkProps = isExistingMovie ? { to: `/movie/${titleSlug}` } : { onPress: this.onPress };

    return (
      <div className={styles.searchResult}>
        <Link
          className={styles.underlay}
          {...linkProps}
        />

        <div className={styles.overlay}>
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

          <div className={styles.content}>
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

              {
                isExclusionMovie &&
                  <Icon
                    className={styles.exclusionIcon}
                    name={icons.DANGER}
                    size={36}
                    title="Movie is on Net Import Exclusion List"
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

              <Tooltip
                anchor={
                  <Label
                    size={sizes.LARGE}
                  >
                    <Icon
                      name={icons.EXTERNAL_LINK}
                      size={13}
                    />

                    <span className={styles.links}>
                      Links
                    </span>
                  </Label>
                }
                tooltip={
                  <MovieDetailsLinks
                    tmdbId={tmdbId}
                    youTubeTrailerId={youTubeTrailerId}
                    imdbId={imdbId}
                  />
                }
                kind={kinds.INVERSE}
                position={tooltipPositions.BOTTOM}
              />

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
        </div>

        <AddNewMovieModal
          isOpen={isNewAddMovieModalOpen && !isExistingMovie}
          tmdbId={tmdbId}
          title={title}
          year={year}
          overview={overview}
          folder={folder}
          images={images}
          onModalClose={this.onAddMovieModalClose}
        />
      </div>
    );
  }
}

AddNewMovieSearchResult.propTypes = {
  tmdbId: PropTypes.number.isRequired,
  imdbId: PropTypes.string,
  youTubeTrailerId: PropTypes.string,
  title: PropTypes.string.isRequired,
  titleSlug: PropTypes.string.isRequired,
  year: PropTypes.number.isRequired,
  studio: PropTypes.string,
  status: PropTypes.string.isRequired,
  overview: PropTypes.string,
  ratings: PropTypes.object.isRequired,
  folder: PropTypes.string.isRequired,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  isExistingMovie: PropTypes.bool.isRequired,
  isExclusionMovie: PropTypes.bool.isRequired,
  isSmallScreen: PropTypes.bool.isRequired
};

export default AddNewMovieSearchResult;
