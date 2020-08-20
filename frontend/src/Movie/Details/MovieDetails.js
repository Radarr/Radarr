import _ from 'lodash';
import moment from 'moment';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { Tab, TabList, TabPanel, Tabs } from 'react-tabs';
import TextTruncate from 'react-text-truncate';
import HeartRating from 'Components/HeartRating';
import Icon from 'Components/Icon';
import InfoLabel from 'Components/InfoLabel';
import IconButton from 'Components/Link/IconButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import Marquee from 'Components/Marquee';
import Measure from 'Components/Measure';
import MonitorToggleButton from 'Components/MonitorToggleButton';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import Popover from 'Components/Tooltip/Popover';
import { icons, kinds, sizes, tooltipPositions } from 'Helpers/Props';
import InteractiveImportModal from 'InteractiveImport/InteractiveImportModal';
import InteractiveSearchFilterMenuConnector from 'InteractiveSearch/InteractiveSearchFilterMenuConnector';
import InteractiveSearchTable from 'InteractiveSearch/InteractiveSearchTable';
import DeleteMovieModal from 'Movie/Delete/DeleteMovieModal';
import EditMovieModalConnector from 'Movie/Edit/EditMovieModalConnector';
import MovieHistoryTable from 'Movie/History/MovieHistoryTable';
import MoviePoster from 'Movie/MoviePoster';
import MovieFileEditorTable from 'MovieFile/Editor/MovieFileEditorTable';
import ExtraFileTable from 'MovieFile/Extras/ExtraFileTable';
import OrganizePreviewModalConnector from 'Organize/OrganizePreviewModalConnector';
import QualityProfileNameConnector from 'Settings/Profiles/Quality/QualityProfileNameConnector';
import fonts from 'Styles/Variables/fonts';
import formatBytes from 'Utilities/Number/formatBytes';
import translate from 'Utilities/String/translate';
import selectAll from 'Utilities/Table/selectAll';
import toggleSelected from 'Utilities/Table/toggleSelected';
import MovieCollectionConnector from './../MovieCollectionConnector';
import MovieCastPostersConnector from './Credits/Cast/MovieCastPostersConnector';
import MovieCrewPostersConnector from './Credits/Crew/MovieCrewPostersConnector';
import MovieDetailsLinks from './MovieDetailsLinks';
import MovieReleaseDatesConnector from './MovieReleaseDatesConnector';
import MovieStatusLabel from './MovieStatusLabel';
import MovieTagsConnector from './MovieTagsConnector';
import MovieTitlesTable from './Titles/MovieTitlesTable';
import styles from './MovieDetails.css';

const defaultFontSize = parseInt(fonts.defaultFontSize);
const lineHeight = parseFloat(fonts.lineHeight);

function getFanartUrl(images) {
  const fanartImage = _.find(images, { coverType: 'fanart' });
  if (fanartImage) {
    // Remove protocol
    return fanartImage.url.replace(/^https?:/, '');
  }
}

function getExpandedState(newState) {
  return {
    allExpanded: newState.allSelected,
    allCollapsed: newState.allUnselected,
    expandedState: newState.selectedState
  };
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
      allExpanded: false,
      allCollapsed: false,
      expandedState: {},
      selectedTabIndex: 0,
      overviewHeight: 0,
      titleWidth: 0
    };
  }

  componentDidMount() {
    window.addEventListener('touchstart', this.onTouchStart);
    window.addEventListener('touchend', this.onTouchEnd);
    window.addEventListener('touchcancel', this.onTouchCancel);
    window.addEventListener('touchmove', this.onTouchMove);
  }

  componentWillUnmount() {
    window.removeEventListener('touchstart', this.onTouchStart);
    window.removeEventListener('touchend', this.onTouchEnd);
    window.removeEventListener('touchcancel', this.onTouchCancel);
    window.removeEventListener('touchmove', this.onTouchMove);
  }

  //
  // Listeners

  onOrganizePress = () => {
    this.setState({ isOrganizeModalOpen: true });
  }

  onOrganizeModalClose = () => {
    this.setState({ isOrganizeModalOpen: false });
  }

  onManageEpisodesPress = () => {
    this.setState({ isManageEpisodesOpen: true });
  }

  onInteractiveImportPress = () => {
    this.setState({ isInteractiveImportModalOpen: true });
  }

  onInteractiveImportModalClose = () => {
    this.setState({ isInteractiveImportModalOpen: false });
  }

  onEditMoviePress = () => {
    this.setState({ isEditMovieModalOpen: true });
  }

  onEditMovieModalClose = () => {
    this.setState({ isEditMovieModalOpen: false });
  }

  onDeleteMoviePress = () => {
    this.setState({
      isEditMovieModalOpen: false,
      isDeleteMovieModalOpen: true
    });
  }

  onDeleteMovieModalClose = () => {
    this.setState({ isDeleteMovieModalOpen: false });
  }

  onExpandAllPress = () => {
    const {
      allExpanded,
      expandedState
    } = this.state;

    this.setState(getExpandedState(selectAll(expandedState, !allExpanded)));
  }

  onExpandPress = (seasonNumber, isExpanded) => {
    this.setState((state) => {
      const convertedState = {
        allSelected: state.allExpanded,
        allUnselected: state.allCollapsed,
        selectedState: state.expandedState
      };

      const newState = toggleSelected(convertedState, [], seasonNumber, isExpanded, false);

      return getExpandedState(newState);
    });
  }

  onMeasure = ({ height }) => {
    this.setState({ overviewHeight: height });
  }

  onTitleMeasure = ({ width }) => {
    this.setState({ titleWidth: width });
  }

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
      this.state.isEventModalOpen
    ) {
      return;
    }

    this._touchStart = touchStart;
  }

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
  }

  onTouchCancel = (event) => {
    this._touchStart = null;
  }

  onTouchMove = (event) => {
    if (!this._touchStart) {
      return;
    }
  }

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
      sizeOnDisk,
      qualityProfileId,
      monitored,
      studio,
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
      isPopulated,
      isSmallScreen,
      movieFilesError,
      movieCreditsError,
      extraFilesError,
      hasMovieFiles,
      previousMovie,
      nextMovie,
      onMonitorTogglePress,
      onRefreshPress,
      onSearchPress
    } = this.props;

    const {
      isOrganizeModalOpen,
      isEditMovieModalOpen,
      isDeleteMovieModalOpen,
      isInteractiveImportModalOpen,
      overviewHeight,
      titleWidth,
      selectedTabIndex
    } = this.state;

    const marqueeWidth = isSmallScreen ? titleWidth : (titleWidth - 150);
    const friendlyRuntime = moment.utc((runtime * 60) * 1000).format('h[h] m[m]');

    return (
      <PageContent title={title}>
        <PageToolbar>
          <PageToolbarSection>
            <PageToolbarButton
              label={translate('RefreshAndScan')}
              iconName={icons.REFRESH}
              spinningName={icons.REFRESH}
              title="Refresh information and scan disk"
              isSpinning={isRefreshing}
              onPress={onRefreshPress}
            />

            <PageToolbarButton
              label={translate('SearchMovie')}
              iconName={icons.SEARCH}
              isDisabled={!monitored}
              isSpinning={isSearching}
              title={undefined}
              onPress={onSearchPress}
            />

            <PageToolbarSeparator />

            <PageToolbarButton
              label={translate('PreviewRename')}
              iconName={icons.ORGANIZE}
              isDisabled={!hasMovieFiles}
              onPress={this.onOrganizePress}
            />

            <PageToolbarButton
              label={translate('ManualImport')}
              iconName={icons.INTERACTIVE}
              onPress={this.onInteractiveImportPress}
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
              style={{
                backgroundImage: `url(${getFanartUrl(images)})`
              }}
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
                        title={`Go to ${previousMovie.title}`}
                        to={`/movie/${previousMovie.titleSlug}`}
                      />

                      <IconButton
                        className={styles.movieNavigationButton}
                        name={icons.ARROW_RIGHT}
                        size={30}
                        title={`Go to ${nextMovie.title}`}
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
                            title="Release Dates"
                            body={
                              <MovieReleaseDatesConnector
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
                          {friendlyRuntime}
                        </span>
                    }

                    {
                      !!ratings &&
                        <span className={styles.rating}>
                          <HeartRating
                            rating={ratings.value}
                            iconSize={20}
                            hideHeart={isSmallScreen}
                          />
                        </span>
                    }

                    {
                      <span className={styles.links}>
                        <Popover
                          anchor={
                            <Icon
                              name={icons.EXTERNAL_LINK}
                              size={20}
                            />
                          }
                          title="Links"
                          body={
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
                        <span className={styles.tags}>
                          <Popover
                            anchor={
                              <Icon
                                name={icons.TAGS}
                                size={20}
                              />
                            }
                            title="Tags"
                            body={
                              <MovieTagsConnector movieId={id} />
                            }
                            position={tooltipPositions.BOTTOM}
                          />
                        </span>
                    }
                  </div>
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
                        formatBytes(sizeOnDisk)
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
                          <MovieCollectionConnector
                            tmdbId={collection.tmdbId}
                            name={collection.name}
                            movieId={id}
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
              !isPopulated && !movieFilesError && !movieCreditsError && !extraFilesError &&
                <LoadingIndicator />
            }

            {
              !isFetching && movieFilesError &&
                <div>Loading movie files failed</div>
            }

            {
              !isFetching && movieCreditsError &&
                <div>Loading movie credits failed</div>
            }

            {
              !isFetching && extraFilesError &&
                <div>Loading movie extra files failed</div>
            }

            <Tabs selectedIndex={this.state.tabIndex} onSelect={(tabIndex) => this.setState({ selectedTabIndex: tabIndex })}>
              <TabList
                className={styles.tabList}
              >
                <Tab
                  className={styles.tab}
                  selectedClassName={styles.selectedTab}
                >
                  {translate('History')}
                </Tab>

                <Tab
                  className={styles.tab}
                  selectedClassName={styles.selectedTab}
                >
                  {translate('Search')}
                </Tab>

                <Tab
                  className={styles.tab}
                  selectedClassName={styles.selectedTab}
                >
                  {translate('Files')}
                </Tab>

                <Tab
                  className={styles.tab}
                  selectedClassName={styles.selectedTab}
                >
                  {translate('Titles')}
                </Tab>

                <Tab
                  className={styles.tab}
                  selectedClassName={styles.selectedTab}
                >
                  {translate('Cast')}
                </Tab>

                <Tab
                  className={styles.tab}
                  selectedClassName={styles.selectedTab}
                >
                  {translate('Crew')}
                </Tab>

                {
                  selectedTabIndex === 1 &&
                    <div className={styles.filterIcon}>
                      <InteractiveSearchFilterMenuConnector />
                    </div>
                }

              </TabList>

              <TabPanel>
                <MovieHistoryTable
                  movieId={id}
                />
              </TabPanel>

              <TabPanel>
                <InteractiveSearchTable
                  movieId={id}
                />
              </TabPanel>

              <TabPanel>
                <MovieFileEditorTable
                  movieId={id}
                />
                <ExtraFileTable
                  movieId={id}
                />
              </TabPanel>

              <TabPanel>
                <MovieTitlesTable
                  movieId={id}
                />
              </TabPanel>

              <TabPanel>
                <MovieCastPostersConnector
                  isSmallScreen={isSmallScreen}
                />
              </TabPanel>

              <TabPanel>
                <MovieCrewPostersConnector
                  isSmallScreen={isSmallScreen}
                />
              </TabPanel>
            </Tabs>

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

          <DeleteMovieModal
            isOpen={isDeleteMovieModalOpen}
            movieId={id}
            onModalClose={this.onDeleteMovieModalClose}
          />

          <InteractiveImportModal
            isOpen={isInteractiveImportModalOpen}
            movieId={id}
            folder={path}
            allowMovieChange={false}
            showFilterExistingFiles={true}
            showImportMode={false}
            onModalClose={this.onInteractiveImportModalClose}
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
  sizeOnDisk: PropTypes.number.isRequired,
  qualityProfileId: PropTypes.number.isRequired,
  monitored: PropTypes.bool.isRequired,
  status: PropTypes.string.isRequired,
  studio: PropTypes.string,
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
  onGoToMovie: PropTypes.func.isRequired
};

MovieDetails.defaultProps = {
  tags: [],
  isSaving: false,
  sizeOnDisk: 0
};

export default MovieDetails;
