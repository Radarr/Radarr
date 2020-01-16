import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import TextTruncate from 'react-text-truncate';
import formatBytes from 'Utilities/Number/formatBytes';
import selectAll from 'Utilities/Table/selectAll';
import toggleSelected from 'Utilities/Table/toggleSelected';
import { icons, kinds, sizes, tooltipPositions } from 'Helpers/Props';
import fonts from 'Styles/Variables/fonts';
import HeartRating from 'Components/HeartRating';
import Icon from 'Components/Icon';
import IconButton from 'Components/Link/IconButton';
import InfoLabel from 'Components/InfoLabel';
import MovieStatusLabel from './MovieStatusLabel';
import Measure from 'Components/Measure';
import MonitorToggleButton from 'Components/MonitorToggleButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBodyConnector from 'Components/Page/PageContentBodyConnector';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import Popover from 'Components/Tooltip/Popover';
import InteractiveImportModal from 'InteractiveImport/InteractiveImportModal';
import MovieFileEditorTable from 'MovieFile/Editor/MovieFileEditorTable';
import OrganizePreviewModalConnector from 'Organize/OrganizePreviewModalConnector';
import QualityProfileNameConnector from 'Settings/Profiles/Quality/QualityProfileNameConnector';
import MoviePoster from 'Movie/MoviePoster';
import EditMovieModalConnector from 'Movie/Edit/EditMovieModalConnector';
import DeleteMovieModal from 'Movie/Delete/DeleteMovieModal';
import MovieHistoryTable from 'Movie/History/MovieHistoryTable';
import MovieTitlesTable from './Titles/MovieTitlesTable';
import MovieCastPostersConnector from './Credits/Cast/MovieCastPostersConnector';
import MovieCrewPostersConnector from './Credits/Crew/MovieCrewPostersConnector';
import MovieAlternateTitles from './MovieAlternateTitles';
import MovieDetailsLinks from './MovieDetailsLinks';
import InteractiveSearchTable from 'InteractiveSearch/InteractiveSearchTable';
import InteractiveSearchFilterMenuConnector from 'InteractiveSearch/InteractiveSearchFilterMenuConnector';
// import MovieTagsConnector from './MovieTagsConnector';
import styles from './MovieDetails.css';
import { Tab, Tabs, TabList, TabPanel } from 'react-tabs';

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
      overviewHeight: 0
    };
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

  //
  // Render

  render() {
    const {
      id,
      tmdbId,
      imdbId,
      title,
      year,
      runtime,
      ratings,
      path,
      sizeOnDisk,
      qualityProfileId,
      monitored,
      studio,
      collection,
      overview,
      youTubeTrailerId,
      inCinemas,
      images,
      alternateTitles,
      // tags,
      isSaving,
      isRefreshing,
      isSearching,
      isFetching,
      isPopulated,
      isSmallScreen,
      movieFilesError,
      movieCreditsError,
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
      selectedTabIndex
    } = this.state;

    return (
      <PageContent title={title}>
        <PageToolbar>
          <PageToolbarSection>
            <PageToolbarButton
              label="Refresh & Scan"
              iconName={icons.REFRESH}
              spinningName={icons.REFRESH}
              title="Refresh information and scan disk"
              isSpinning={isRefreshing}
              onPress={onRefreshPress}
            />

            <PageToolbarButton
              label="Search Movie"
              iconName={icons.SEARCH}
              isDisabled={!monitored}
              isSpinning={isSearching}
              title={undefined}
              onPress={onSearchPress}
            />

            <PageToolbarSeparator />

            <PageToolbarButton
              label="Preview Rename"
              iconName={icons.ORGANIZE}
              isDisabled={!hasMovieFiles}
              onPress={this.onOrganizePress}
            />

            <PageToolbarButton
              label="Manual Import"
              iconName={icons.INTERACTIVE}
              onPress={this.onInteractiveImportPress}
            />

            <PageToolbarSeparator />

            <PageToolbarButton
              label="Edit"
              iconName={icons.EDIT}
              onPress={this.onEditMoviePress}
            />

            <PageToolbarButton
              label="Delete"
              iconName={icons.DELETE}
              onPress={this.onDeleteMoviePress}
            />
          </PageToolbarSection>
        </PageToolbar>

        <PageContentBodyConnector innerClassName={styles.innerContentBody}>
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

                    <div className={styles.title}>
                      {title}
                    </div>

                    <div className={styles.year}>
                      {year}
                    </div>

                    {
                      !!alternateTitles.length &&
                        <div className={styles.alternateTitlesIconContainer}>
                          <Popover
                            anchor={
                              <Icon
                                name={icons.ALTERNATE_TITLES}
                                size={20}
                              />
                            }
                            title="Alternate Titles"
                            body={<MovieAlternateTitles alternateTitles={alternateTitles} />}
                            position={tooltipPositions.BOTTOM}
                          />
                        </div>
                    }
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

                <div className={styles.details}>
                  <div>
                    {
                      !!runtime &&
                        <span className={styles.runtime}>
                          {runtime} Minutes
                        </span>
                    }

                    <HeartRating
                      rating={ratings.value}
                      iconSize={20}
                    />
                  </div>
                </div>

                <div className={styles.detailsLabels}>
                  <InfoLabel
                    className={styles.detailsInfoLabel}
                    title="Path"
                    size={sizes.LARGE}
                  >
                    <span className={styles.path}>
                      {path}
                    </span>
                  </InfoLabel>

                  <InfoLabel
                    className={styles.detailsInfoLabel}
                    title="Status"
                    kind={kinds.DELETE}
                    size={sizes.LARGE}
                  >
                    <span className={styles.statusName}>
                      <MovieStatusLabel
                        hasMovieFiles={hasMovieFiles}
                        monitored={monitored}
                        inCinemas={inCinemas}
                      />
                    </span>
                  </InfoLabel>

                  <InfoLabel
                    className={styles.detailsInfoLabel}
                    title="Quality Profile"
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
                    title="Filesize"
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
                        title="Collection"
                        size={sizes.LARGE}
                      >
                        <span className={styles.collection}>
                          {collection.name}
                        </span>
                      </InfoLabel>
                  }

                  {
                    !!studio &&
                      <InfoLabel
                        className={styles.detailsInfoLabel}
                        title="Studio"
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

                <InfoLabel
                  className={styles.detailsInfoLabel}
                  title="Path"
                  size={sizes.LARGE}
                >
                  <span className={styles.path}>
                    {path}
                  </span>
                </InfoLabel>

                <InfoLabel
                  className={styles.detailsInfoLabel}
                  title="Links"
                  size={sizes.LARGE}
                >
                  <span className={styles.links}>
                    {
                      <MovieDetailsLinks
                        tmdbId={tmdbId}
                        imdbId={imdbId}
                        youTubeTrailerId={youTubeTrailerId}
                      />
                    }
                  </span>
                </InfoLabel>
              </div>
            </div>
          </div>

          <div className={styles.contentContainer}>
            {
              !isPopulated && !movieFilesError && !movieCreditsError &&
                <LoadingIndicator />
            }

            {
              !isFetching && movieFilesError && !movieCreditsError &&
                <div>Loading movie files failed</div>
            }

            <Tabs selectedIndex={this.state.tabIndex} onSelect={(tabIndex) => this.setState({ selectedTabIndex: tabIndex })}>
              <TabList
                className={styles.tabList}
              >
                <Tab
                  className={styles.tab}
                  selectedClassName={styles.selectedTab}
                >
                  History
                </Tab>

                <Tab
                  className={styles.tab}
                  selectedClassName={styles.selectedTab}
                >
                  Search
                </Tab>

                <Tab
                  className={styles.tab}
                  selectedClassName={styles.selectedTab}
                >
                  Files
                </Tab>

                <Tab
                  className={styles.tab}
                  selectedClassName={styles.selectedTab}
                >
                  Titles
                </Tab>

                <Tab
                  className={styles.tab}
                  selectedClassName={styles.selectedTab}
                >
                  Cast
                </Tab>

                <Tab
                  className={styles.tab}
                  selectedClassName={styles.selectedTab}
                >
                  Crew
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
        </PageContentBodyConnector>
      </PageContent>
    );
  }
}

MovieDetails.propTypes = {
  id: PropTypes.number.isRequired,
  tmdbId: PropTypes.number.isRequired,
  imdbId: PropTypes.string,
  title: PropTypes.string.isRequired,
  year: PropTypes.number.isRequired,
  runtime: PropTypes.number.isRequired,
  ratings: PropTypes.object.isRequired,
  path: PropTypes.string.isRequired,
  sizeOnDisk: PropTypes.number.isRequired,
  qualityProfileId: PropTypes.number.isRequired,
  monitored: PropTypes.bool.isRequired,
  status: PropTypes.string.isRequired,
  studio: PropTypes.string,
  collection: PropTypes.object,
  youTubeTrailerId: PropTypes.string,
  inCinemas: PropTypes.string.isRequired,
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
  movieFilesError: PropTypes.object,
  movieCreditsError: PropTypes.object,
  hasMovieFiles: PropTypes.bool.isRequired,
  previousMovie: PropTypes.object.isRequired,
  nextMovie: PropTypes.object.isRequired,
  onMonitorTogglePress: PropTypes.func.isRequired,
  onRefreshPress: PropTypes.func.isRequired,
  onSearchPress: PropTypes.func.isRequired
};

MovieDetails.defaultProps = {
  tags: [],
  isSaving: false,
  sizeOnDisk: 0
};

export default MovieDetails;
