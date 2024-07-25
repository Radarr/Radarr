import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Icon from 'Components/Icon';
import ImdbRating from 'Components/ImdbRating';
import Label from 'Components/Label';
import Link from 'Components/Link/Link';
import TmdbRating from 'Components/TmdbRating';
import Tooltip from 'Components/Tooltip/Tooltip';
import { icons, kinds, sizes, tooltipPositions } from 'Helpers/Props';
import MovieDetailsLinks from 'Movie/Details/MovieDetailsLinks';
import MovieStatusLabel from 'Movie/Details/MovieStatusLabel';
import MovieIndexProgressBar from 'Movie/Index/ProgressBar/MovieIndexProgressBar';
import MoviePoster from 'Movie/MoviePoster';
import formatRuntime from 'Utilities/Date/formatRuntime';
import translate from 'Utilities/String/translate';
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
  };

  onAddMovieModalClose = () => {
    this.setState({ isNewAddMovieModalOpen: false });
  };

  onExternalLinkPress = (event) => {
    event.stopPropagation();
  };

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
      originalLanguage,
      genres,
      status,
      overview,
      ratings,
      folder,
      images,
      existingMovieId,
      isExistingMovie,
      isExclusionMovie,
      isSmallScreen,
      colorImpairedMode,
      id,
      monitored,
      isAvailable,
      movieFile,
      queueItem,
      runtime,
      movieRuntimeFormat,
      certification
    } = this.props;

    const {
      isNewAddMovieModalOpen
    } = this.state;

    const hasMovieFile = !!movieFile;

    const linkProps = isExistingMovie ? { to: `/movie/${titleSlug}` } : { onPress: this.onPress };
    const posterWidth = 167;
    const posterHeight = 250;

    const elementStyle = {
      width: `${posterWidth}px`,
      height: `${posterHeight}px`
    };

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
              <div>
                <div className={styles.posterContainer}>
                  <MoviePoster
                    className={styles.poster}
                    style={elementStyle}
                    images={images}
                    size={250}
                    overflow={true}
                    lazy={false}
                  />
                </div>

                {
                  isExistingMovie &&
                    <MovieIndexProgressBar
                      movieId={existingMovieId}
                      movieFile={movieFile}
                      monitored={monitored}
                      hasFile={hasMovieFile}
                      status={status}
                      width={posterWidth}
                      detailedProgressBar={true}
                      isAvailable={isAvailable}
                    />
                }
              </div>
          }

          <div className={styles.content}>
            <div className={styles.titleRow}>
              <div className={styles.titleContainer}>
                <div className={styles.title}>
                  {title}

                  {
                    !title.contains(year) && !!year ?
                      <span className={styles.year}>
                        ({year})
                      </span> :
                      null
                  }
                </div>
              </div>

              <div className={styles.icons}>

                {
                  isExistingMovie &&
                    <Icon
                      className={styles.alreadyExistsIcon}
                      name={icons.CHECK_CIRCLE}
                      size={36}
                      title={translate('AlreadyInYourLibrary')}
                    />
                }

                {
                  isExclusionMovie &&
                    <Icon
                      className={styles.exclusionIcon}
                      name={icons.DANGER}
                      size={36}
                      title={translate('MovieIsOnImportExclusionList')}
                    />
                }
              </div>
            </div>

            <div>
              {
                !!certification &&
                  <span className={styles.certification}>
                    {certification}
                  </span>
              }

              {
                !!runtime &&
                  <span className={styles.runtime}>
                    {formatRuntime(runtime, movieRuntimeFormat)}
                  </span>
              }
            </div>

            <div>
              <Label size={sizes.LARGE}>
                <TmdbRating
                  ratings={ratings}
                  iconSize={13}
                />
              </Label>

              {
                ratings.imdb ?
                  <Label size={sizes.LARGE}>
                    <ImdbRating
                      ratings={ratings}
                      iconSize={13}
                    />
                  </Label> :
                  null
              }

              {
                originalLanguage?.name ?
                  <Label size={sizes.LARGE}>
                    <Icon
                      name={icons.LANGUAGE}
                      size={13}
                    />
                    <span className={styles.originalLanguage}>
                      {originalLanguage.name}
                    </span>
                  </Label> :
                  null
              }

              {
                studio ?
                  <Label size={sizes.LARGE}>
                    <Icon
                      name={icons.STUDIO}
                      size={13}
                    />
                    <span className={styles.studio}>
                      {studio}
                    </span>
                  </Label> :
                  null
              }

              {
                genres.length > 0 ?
                  <Label size={sizes.LARGE}>
                    <Icon
                      name={icons.GENRE}
                      size={13}
                    />
                    <span className={styles.genres}>
                      {genres.slice(0, 3).join(', ')}
                    </span>
                  </Label> :
                  null
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
                      {translate('Links')}
                    </span>
                  </Label>
                }
                tooltip={
                  <MovieDetailsLinks
                    tmdbId={tmdbId}
                    imdbId={imdbId}
                    youTubeTrailerId={youTubeTrailerId}
                  />
                }
                canFlip={true}
                kind={kinds.INVERSE}
                position={tooltipPositions.BOTTOM}
              />

              {
                isExistingMovie && isSmallScreen &&
                  <MovieStatusLabel
                    hasMovieFiles={hasMovieFile}
                    monitored={monitored}
                    isAvailable={isAvailable}
                    queueItem={queueItem}
                    id={id}
                    useLabel={true}
                    colorImpairedMode={colorImpairedMode}
                  />
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
  originalLanguage: PropTypes.object,
  genres: PropTypes.arrayOf(PropTypes.string),
  status: PropTypes.string.isRequired,
  overview: PropTypes.string,
  ratings: PropTypes.object.isRequired,
  folder: PropTypes.string.isRequired,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  existingMovieId: PropTypes.number,
  isExistingMovie: PropTypes.bool.isRequired,
  isExclusionMovie: PropTypes.bool.isRequired,
  isSmallScreen: PropTypes.bool.isRequired,
  id: PropTypes.number,
  monitored: PropTypes.bool.isRequired,
  isAvailable: PropTypes.bool.isRequired,
  movieFile: PropTypes.object,
  queueItem: PropTypes.object,
  colorImpairedMode: PropTypes.bool,
  runtime: PropTypes.number.isRequired,
  movieRuntimeFormat: PropTypes.string.isRequired,
  certification: PropTypes.string
};

AddNewMovieSearchResult.defaultProps = {
  genres: []
};

export default AddNewMovieSearchResult;
