import PropTypes from 'prop-types';
import React, { Component } from 'react';
import TextTruncate from 'react-text-truncate';
import Alert from 'Components/Alert';
import FieldSet from 'Components/FieldSet';
import Icon from 'Components/Icon';
import ImdbRating from 'Components/ImdbRating';
import InfoLabel from 'Components/InfoLabel';
import IconButton from 'Components/Link/IconButton';
import Marquee from 'Components/Marquee';
import Measure from 'Components/Measure';
import MonitorToggleButton from 'Components/MonitorToggleButton';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import RottenTomatoRating from 'Components/RottenTomatoRating';
import TmdbRating from 'Components/TmdbRating';
import Popover from 'Components/Tooltip/Popover';
import Tooltip from 'Components/Tooltip/Tooltip';
import { icons, kinds, sizes, tooltipPositions } from 'Helpers/Props';
import InteractiveImportModal from 'InteractiveImport/InteractiveImportModal';
import DeleteMovieModal from 'Movie/Delete/DeleteMovieModal';
import EditMovieModalConnector from 'Movie/Edit/EditMovieModalConnector';
import MovieHistoryModal from 'Movie/History/MovieHistoryModal';
import MoviePoster from 'Movie/MoviePoster';
import MovieInteractiveSearchModalConnector from 'Movie/Search/MovieInteractiveSearchModalConnector';
import MovieFileEditorTable from 'MovieFile/Editor/MovieFileEditorTable';
import ExtraFileTable from 'MovieFile/Extras/ExtraFileTable';
import OrganizePreviewModalConnector from 'Organize/OrganizePreviewModalConnector';
import QualityProfileNameConnector from 'Settings/Profiles/Quality/QualityProfileNameConnector';
import fonts from 'Styles/Variables/fonts';
import * as keyCodes from 'Utilities/Constants/keyCodes';
import formatRuntime from 'Utilities/Date/formatRuntime';
import formatBytes from 'Utilities/Number/formatBytes';
import translate from 'Utilities/String/translate';
import MovieCollectionLabelConnector from './../MovieCollectionLabelConnector';
import MovieCastPostersConnector from './Credits/Cast/MovieCastPostersConnector';
import MovieCrewPostersConnector from './Credits/Crew/MovieCrewPostersConnector';
import MovieDetailsLinks from './MovieDetailsLinks';
import MovieReleaseDates from './MovieReleaseDates';
import MovieStatusLabel from './MovieStatusLabel';
import MovieTagsConnector from './MovieTagsConnector';
import MovieTitlesTable from './Titles/MovieTitlesTable';
import styles from './MovieDetails.css';

const defaultFontSize = parseInt(fonts.defaultFontSize);
const lineHeight = parseFloat(fonts.lineHeight);

function getFanartUrl(images) {
  const image = images.find((img) => img.coverType === 'fanart');
  return image?.url ?? image?.remoteUrl;
}

class MovieDetails extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isOrganizeModalOpen: false,
      isEditMovieModalOpen: false,
      isDeleteMovieModalOpen: false,
      isInteractiveImportModalOpen: false,
      isInteractiveSearchModalOpen: false,
      isMovieHistoryModalOpen: false,
      overviewHeight: 0,
      titleWidth: 0
    };
  }

  componentDidMount() {
    window.addEventListener('touchstart', this.onTouchStart);
    window.addEventListener('touchend', this.onTouchEnd);
    window.addEventListener('touchcancel', this.onTouchCancel);
    window.addEventListener('touchmove', this.onTouchMove);
    window.addEventListener('keyup', this.onKeyUp);
  }

  componentWillUnmount() {
    window.removeEventListener('touchstart', this.onTouchStart);
    window.removeEventListener('touchend', this.onTouchEnd);
    window.removeEventListener('touchcancel', this.onTouchCancel);
    window.removeEventListener('touchmove', this.onTouchMove);
    window.removeEventListener('keyup', this.onKeyUp);
  }

  //
  // Listeners

  onOrganizePress = () => {
    this.setState({ isOrganizeModalOpen: true });
  };

  onOrganizeModalClose = () => {
    this.setState({ isOrganizeModalOpen: false });
  };

  onInteractiveImportPress = () => {
    this.setState({ isInteractiveImportModalOpen: true });
  };

  onInteractiveImportModalClose = () => {
    this.setState({ isInteractiveImportModalOpen: false });
  };

  onEditMoviePress = () => {
    this.setState({ isEditMovieModalOpen: true });
  };

  onEditMovieModalClose = () => {
    this.setState({ isEditMovieModalOpen: false });
  };

  onInteractiveSearchPress = () => {
    this.setState({ isInteractiveSearchModalOpen: true });
  };

  onInteractiveSearchModalClose = () => {
    this.setState({ isInteractiveSearchModalOpen: false });
  };

  onDeleteMoviePress = () => {
    this.setState({
      isEditMovieModalOpen: false,
      isDeleteMovieModalOpen: true
    });
  };

  onDeleteMovieModalClose = () => {
    this.setState({ isDeleteMovieModalOpen: false });
  };

  onMovieHistoryPress = () => {
    this.setState({ isMovieHistoryModalOpen: true });
  };

  onMovieHistoryModalClose = () => {
    this.setState({ isMovieHistoryModalOpen: false });
  };

  onMeasure = ({ height }) => {
    this.setState({ overviewHeight: height });
  };

  onTitleMeasure = ({ width }) => {
    this.setState({ titleWidth: width });
  };

  onKeyUp = (event) => {
    if (event.composedPath && event.composedPath().length === 4) {
      if (event.keyCode === keyCodes.LEFT_ARROW) {
        this.props.onGoToMovie(this.props.previousMovie.titleSlug);
      }
      if (event.keyCode === keyCodes.RIGHT_ARROW) {
        this.props.onGoToMovie(this.props.nextMovie.titleSlug);
      }
    }
  };

  onTouchStart = (event) => {
    const touches = event.touches;
    const touchStart = touches[0].pageX;
    const touchY = touches[0].pageY;

    // Only change when swipe is on header, we need horizontal scroll on tables
    if (touchY > 470) {
      return;
    }

    if (touches.length !== 1) {
      return;
    }

    if (
      touchStart < 50 ||
      this.props.isSidebarVisible ||
      this.state.isOrganizeModalOpen ||
      this.state.isEditMovieModalOpen ||
      this.state.isDeleteMovieModalOpen ||
      this.state.isInteractiveImportModalOpen ||
      this.state.isInteractiveSearchModalOpen ||
      this.state.isMovieHistoryModalOpen
    ) {
      return;
    }

    this._touchStart = touchStart;
  };

  onTouchEnd = (event) => {
    const touches = event.changedTouches;
    const currentTouch = touches[0].pageX;

    if (!this._touchStart) {
      return;
    }

    if (currentTouch > this._touchStart && currentTouch - this._touchStart > 100) {
      this.props.onGoToMovie(this.props.previousMovie.titleSlug);
    } else if (currentTouch < this._touchStart && this._touchStart - currentTouch > 100) {
      this.props.onGoToMovie(this.props.nextMovie.titleSlug);
    }

    this._touchStart = null;
  };

  onTouchCancel = (event) => {
    this._touchStart = null;
  };

  onTouchMove = (event) => {
    if (!this._touchStart) {
      return;
    }
  };

  //
  // Render

  render() {
    const {
      id,
      tmdbId,
      imdbId,
      title,
      originalTitle,
      year,
      inCinemas,
      physicalRelease,
      digitalRelease,
      runtime,
      certification,
      ratings,
      path,
      statistics,
      qualityProfileId,
      monitored,
      studio,
      genres,
      collection,
      overview,
      youTubeTrailerId,
      isAvailable,
      images,
      tags,
      isSaving,
      isRefreshing,
      isSearching,
      isFetching,
      isSmallScreen,
      movieFilesError,
      movieCreditsError,
      extraFilesError,
      hasMovieFiles,
      previousMovie,
      nextMovie,
      onMonitorTogglePress,
      onRefreshPress,
      onSearchPress,
      queueItem,
      movieRuntimeFormat
    } = this.props;

    const {
      sizeOnDisk = 0
    } = statistics;

    const {
      isOrganizeModalOpen,
      isEditMovieModalOpen,
      isDeleteMovieModalOpen,
      isInteractiveImportModalOpen,
      isInteractiveSearchModalOpen,
      isMovieHistoryModalOpen,
      overviewHeight,
      titleWidth
    } = this.state;

    const fanartUrl = getFanartUrl(images);
    const marqueeWidth = isSmallScreen ? titleWidth : (titleWidth - 150);

    const pageTitle = `${title}${year > 0 ? ` (${year})` : ''}`;

    return (
      <PageContent title={pageTitle}>
        <PageToolbar>
          <PageToolbarSection>
            <PageToolbarButton
              label={translate('RefreshAndScan')}
              iconName={icons.REFRESH}
              spinningName={icons.REFRESH}
              title={translate('RefreshInformationAndScanDisk')}
              isSpinning={isRefreshing}
              onPress={onRefreshPress}
            />

            <PageToolbarButton
              label={translate('SearchMovie')}
              iconName={icons.SEARCH}
              isSpinning={isSearching}
              title={undefined}
              onPress={onSearchPress}
            />

            <PageToolbarButton
              label={translate('InteractiveSearch')}
              iconName={icons.INTERACTIVE}
              isSpinning={isSearching}
              title={undefined}
              onPress={this.onInteractiveSearchPress}
            />

            <PageToolbarSeparator />

            <PageToolbarButton
              label={translate('PreviewRename')}
              iconName={icons.ORGANIZE}
              isDisabled={!hasMovieFiles}
              onPress={this.onOrganizePress}
            />

            <PageToolbarButton
              label={translate('ManageFiles')}
              iconName={icons.MOVIE_FILE}
              onPress={this.onInteractiveImportPress}
            />

            <PageToolbarButton
              label={translate('History')}
              iconName={icons.HISTORY}
              onPress={this.onMovieHistoryPress}
            />

            <PageToolbarSeparator />

            <PageToolbarButton
              label={translate('Edit')}
              iconName={icons.EDIT}
              onPress={this.onEditMoviePress}
            />

            <PageToolbarButton
              label={translate('Delete')}
              iconName={icons.DELETE}
              onPress={this.onDeleteMoviePress}
            />
          </PageToolbarSection>
        </PageToolbar>

        <PageContentBody innerClassName={styles.innerContentBody}>
          <div className={styles.header}>
            <div
              className={styles.backdrop}
              style={
                fanartUrl ?
                  { backgroundImage: `url(${fanartUrl})` } :
                  null
              }
            >
              <div className={styles.backdropOverlay} />
            </div>

            <div className={styles.headerContent}>
              <MoviePoster
                className={styles.poster}
                images={images}
                size={250}
                lazy={false}
              />

              <div className={styles.info}>
                <Measure onMeasure={this.onTitleMeasure}>
                  <div className={styles.titleRow}>
                    <div className={styles.titleContainer}>
                      <div className={styles.toggleMonitoredContainer}>
                        <MonitorToggleButton
                          className={styles.monitorToggleButton}
                          monitored={monitored}
                          isSaving={isSaving}
                          size={40}
                          onPress={onMonitorTogglePress}
                        />
                      </div>

                      <div className={styles.title} style={{ width: marqueeWidth }}>
                        <Marquee text={title} title={originalTitle} />
                      </div>
                    </div>

                    <div className={styles.movieNavigationButtons}>
                      <IconButton
                        className={styles.movieNavigationButton}
                        name={icons.ARROW_LEFT}
                        size={30}
                        title={translate('GoToInterp', [previousMovie.title])}
                        to={`/movie/${previousMovie.titleSlug}`}
                      />

                      <IconButton
                        className={styles.movieNavigationButton}
                        name={icons.ARROW_RIGHT}
                        size={30}
                        title={translate('GoToInterp', [nextMovie.title])}
                        to={`/movie/${nextMovie.titleSlug}`}
                      />
                    </div>
                  </div>
                </Measure>

                <div className={styles.details}>
                  <div>
                    {
                      !!certification &&
                        <span className={styles.certification}>
                          {certification}
                        </span>
                    }

                    {
                      year > 0 &&
                        <span className={styles.year}>
                          <Popover
                            anchor={
                              year
                            }
                            title={translate('ReleaseDates')}
                            body={
                              <MovieReleaseDates
                                inCinemas={inCinemas}
                                physicalRelease={physicalRelease}
                                digitalRelease={digitalRelease}
                              />
                            }
                            position={tooltipPositions.BOTTOM}
                          />
                        </span>
                    }

                    {
                      !!runtime &&
                        <span className={styles.runtime}>
                          {formatRuntime(runtime, movieRuntimeFormat)}
                        </span>
                    }

                    {
                      <span className={styles.links}>
                        <Tooltip
                          anchor={
                            <Icon
                              name={icons.EXTERNAL_LINK}
                              size={20}
                            />
                          }
                          tooltip={
                            <MovieDetailsLinks
                              tmdbId={tmdbId}
                              imdbId={imdbId}
                              youTubeTrailerId={youTubeTrailerId}
                            />
                          }
                          position={tooltipPositions.BOTTOM}
                        />
                      </span>
                    }

                    {
                      !!tags.length &&
                        <span>
                          <Tooltip
                            anchor={
                              <Icon
                                name={icons.TAGS}
                                size={20}
                              />
                            }
                            tooltip={
                              <MovieTagsConnector movieId={id} />
                            }
                            position={tooltipPositions.BOTTOM}
                          />
                        </span>
                    }
                  </div>
                </div>

                <div className={styles.details}>
                  {
                    !!ratings.tmdb &&
                      <span className={styles.rating}>
                        <TmdbRating
                          ratings={ratings}
                          iconSize={20}
                        />
                      </span>
                  }
                  {
                    !!ratings.imdb &&
                      <span className={styles.rating}>
                        <ImdbRating
                          ratings={ratings}
                          iconSize={20}
                        />
                      </span>
                  }
                  {
                    !!ratings.rottenTomatoes &&
                      <span className={styles.rating}>
                        <RottenTomatoRating
                          ratings={ratings}
                          iconSize={20}
                        />
                      </span>
                  }
                </div>

                <div className={styles.detailsLabels}>
                  <InfoLabel
                    className={styles.detailsInfoLabel}
                    title={translate('Path')}
                    size={sizes.LARGE}
                  >
                    <span className={styles.path}>
                      {path}
                    </span>
                  </InfoLabel>

                  <InfoLabel
                    className={styles.detailsInfoLabel}
                    title={translate('Status')}
                    kind={kinds.DELETE}
                    size={sizes.LARGE}
                  >
                    <span className={styles.statusName}>
                      <MovieStatusLabel
                        hasMovieFiles={hasMovieFiles}
                        monitored={monitored}
                        isAvailable={isAvailable}
                        queueItem={queueItem}
                      />
                    </span>
                  </InfoLabel>

                  <InfoLabel
                    className={styles.detailsInfoLabel}
                    title={translate('QualityProfile')}
                    size={sizes.LARGE}
                  >
                    <span className={styles.qualityProfileName}>
                      {
                        <QualityProfileNameConnector
                          qualityProfileId={qualityProfileId}
                        />
                      }
                    </span>
                  </InfoLabel>

                  <InfoLabel
                    className={styles.detailsInfoLabel}
                    title={translate('Size')}
                    size={sizes.LARGE}
                  >
                    <span className={styles.sizeOnDisk}>
                      {
                        formatBytes(sizeOnDisk || 0)
                      }
                    </span>
                  </InfoLabel>

                  {
                    !!collection &&
                      <InfoLabel
                        className={styles.detailsInfoLabel}
                        title={translate('Collection')}
                        size={sizes.LARGE}
                      >
                        <div className={styles.collection}>
                          <MovieCollectionLabelConnector
                            tmdbId={collection.tmdbId}
                          />
                        </div>
                      </InfoLabel>
                  }

                  {
                    !!studio && !isSmallScreen &&
                      <InfoLabel
                        className={styles.detailsInfoLabel}
                        title={translate('Studio')}
                        size={sizes.LARGE}
                      >
                        <span className={styles.studio}>
                          {studio}
                        </span>
                      </InfoLabel>
                  }

                  {
                    !!genres.length && !isSmallScreen &&
                      <InfoLabel
                        className={styles.detailsInfoLabel}
                        title={translate('Genres')}
                        size={sizes.LARGE}
                      >
                        <span className={styles.genres}>
                          {genres.join(', ')}
                        </span>
                      </InfoLabel>
                  }
                </div>

                <Measure onMeasure={this.onMeasure}>
                  <div className={styles.overview}>
                    <TextTruncate
                      line={Math.floor(overviewHeight / (defaultFontSize * lineHeight))}
                      text={overview}
                    />
                  </div>
                </Measure>
              </div>
            </div>
          </div>

          <div className={styles.contentContainer}>
            {
              !isFetching && movieFilesError ?
                <Alert kind={kinds.DANGER}>
                  {translate('LoadingMovieFilesFailed')}
                </Alert> :
                null
            }

            {
              !isFetching && movieCreditsError ?
                <Alert kind={kinds.DANGER}>
                  {translate('LoadingMovieCreditsFailed')}
                </Alert> :
                null
            }

            {
              !isFetching && extraFilesError ?
                <Alert kind={kinds.DANGER}>
                  {translate('LoadingMovieExtraFilesFailed')}
                </Alert> :
                null
            }

            <FieldSet legend={translate('Files')}>
              <MovieFileEditorTable
                movieId={id}
              />

              <ExtraFileTable
                movieId={id}
              />
            </FieldSet>

            <FieldSet legend={translate('Cast')}>
              <MovieCastPostersConnector
                isSmallScreen={isSmallScreen}
              />
            </FieldSet>

            <FieldSet legend={translate('Crew')}>
              <MovieCrewPostersConnector
                isSmallScreen={isSmallScreen}
              />
            </FieldSet>

            <FieldSet legend={translate('Titles')}>
              <MovieTitlesTable
                movieId={id}
              />
            </FieldSet>
          </div>

          <OrganizePreviewModalConnector
            isOpen={isOrganizeModalOpen}
            movieId={id}
            onModalClose={this.onOrganizeModalClose}
          />

          <EditMovieModalConnector
            isOpen={isEditMovieModalOpen}
            movieId={id}
            onModalClose={this.onEditMovieModalClose}
            onDeleteMoviePress={this.onDeleteMoviePress}
          />

          <MovieHistoryModal
            isOpen={isMovieHistoryModalOpen}
            movieId={id}
            onModalClose={this.onMovieHistoryModalClose}
          />

          <DeleteMovieModal
            isOpen={isDeleteMovieModalOpen}
            movieId={id}
            onModalClose={this.onDeleteMovieModalClose}
            nextMovieRelativePath={`/movie/${nextMovie.titleSlug}`}
          />

          <InteractiveImportModal
            isOpen={isInteractiveImportModalOpen}
            movieId={id}
            modalTitle={translate('ManageFiles')}
            folder={path}
            allowMovieChange={false}
            showFilterExistingFiles={true}
            showImportMode={false}
            onModalClose={this.onInteractiveImportModalClose}
          />

          <MovieInteractiveSearchModalConnector
            isOpen={isInteractiveSearchModalOpen}
            movieId={id}
            onModalClose={this.onInteractiveSearchModalClose}
          />
        </PageContentBody>
      </PageContent>
    );
  }
}

MovieDetails.propTypes = {
  id: PropTypes.number.isRequired,
  tmdbId: PropTypes.number.isRequired,
  imdbId: PropTypes.string,
  title: PropTypes.string.isRequired,
  originalTitle: PropTypes.string,
  year: PropTypes.number.isRequired,
  runtime: PropTypes.number.isRequired,
  certification: PropTypes.string,
  ratings: PropTypes.object.isRequired,
  path: PropTypes.string.isRequired,
  statistics: PropTypes.object.isRequired,
  qualityProfileId: PropTypes.number.isRequired,
  monitored: PropTypes.bool.isRequired,
  status: PropTypes.string.isRequired,
  studio: PropTypes.string,
  genres: PropTypes.arrayOf(PropTypes.string).isRequired,
  collection: PropTypes.object,
  youTubeTrailerId: PropTypes.string,
  isAvailable: PropTypes.bool.isRequired,
  inCinemas: PropTypes.string,
  physicalRelease: PropTypes.string,
  digitalRelease: PropTypes.string,
  overview: PropTypes.string.isRequired,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  alternateTitles: PropTypes.arrayOf(PropTypes.string).isRequired,
  tags: PropTypes.arrayOf(PropTypes.number).isRequired,
  isSaving: PropTypes.bool.isRequired,
  isRefreshing: PropTypes.bool.isRequired,
  isSearching: PropTypes.bool.isRequired,
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  isSmallScreen: PropTypes.bool.isRequired,
  isSidebarVisible: PropTypes.bool.isRequired,
  movieFilesError: PropTypes.object,
  movieCreditsError: PropTypes.object,
  extraFilesError: PropTypes.object,
  hasMovieFiles: PropTypes.bool.isRequired,
  previousMovie: PropTypes.object.isRequired,
  nextMovie: PropTypes.object.isRequired,
  onMonitorTogglePress: PropTypes.func.isRequired,
  onRefreshPress: PropTypes.func.isRequired,
  onSearchPress: PropTypes.func.isRequired,
  onGoToMovie: PropTypes.func.isRequired,
  queueItem: PropTypes.object,
  movieRuntimeFormat: PropTypes.string.isRequired
};

MovieDetails.defaultProps = {
  genres: [],
  statistics: {},
  tags: [],
  isSaving: false
};

export default MovieDetails;
