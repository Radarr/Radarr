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
import Label from 'Components/Label';
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
import Tooltip from 'Components/Tooltip/Tooltip';
import MovieFileEditorModal from 'MovieFile/Editor/MovieFileEditorModal';
import OrganizePreviewModalConnector from 'Organize/OrganizePreviewModalConnector';
import QualityProfileNameConnector from 'Settings/Profiles/Quality/QualityProfileNameConnector';
import MoviePoster from 'Movie/MoviePoster';
import EditMovieModalConnector from 'Movie/Edit/EditMovieModalConnector';
import DeleteMovieModal from 'Movie/Delete/DeleteMovieModal';
import MovieHistoryModal from 'Movie/History/MovieHistoryModal';
import MovieAlternateTitles from './MovieAlternateTitles';
import MovieTagsConnector from './MovieTagsConnector';
import MovieDetailsLinks from './MovieDetailsLinks';
import styles from './MovieDetails.css';
import InteractiveImportModal from '../../InteractiveImport/InteractiveImportModal';

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
      isManageEpisodesOpen: false,
      isEditMovieModalOpen: false,
      isDeleteMovieModalOpen: false,
      isMovieHistoryModalOpen: false,
      isInteractiveImportModalOpen: false,
      allExpanded: false,
      allCollapsed: false,
      expandedState: {},
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

  onManageEpisodesModalClose = () => {
    this.setState({ isManageEpisodesOpen: false });
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

  onMovieHistoryPress = () => {
    this.setState({ isMovieHistoryModalOpen: true });
  }

  onMovieHistoryModalClose = () => {
    this.setState({ isMovieHistoryModalOpen: false });
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
      runtime,
      ratings,
      path,
      sizeOnDisk,
      qualityProfileId,
      monitored,
      studio,
      overview,
      images,
      alternateTitles,
      tags,
      isSaving,
      isRefreshing,
      isSearching,
      isFetching,
      isPopulated,
      movieFilesError,
      hasMovieFiles,
      previousMovie,
      nextMovie,
      onMonitorTogglePress,
      onRefreshPress,
      onSearchPress
    } = this.props;

    const {
      isOrganizeModalOpen,
      isManageEpisodesOpen,
      isEditMovieModalOpen,
      isDeleteMovieModalOpen,
      isMovieHistoryModalOpen,
      isInteractiveImportModalOpen,
      overviewHeight
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
              label="Manage Files"
              iconName={icons.MOVIE_FILE}
              isDisabled={!hasMovieFiles}
              onPress={this.onManageEpisodesPress}
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
                size={500}
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

                  <div className={styles.seriesNavigationButtons}>
                    <IconButton
                      className={styles.seriesNavigationButton}
                      name={icons.ARROW_LEFT}
                      size={30}
                      title={`Go to ${previousMovie.title}`}
                      to={`/movie/${previousMovie.titleSlug}`}
                    />

                    <IconButton
                      className={styles.seriesNavigationButton}
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
                  <Label
                    className={styles.detailsLabel}
                    size={sizes.LARGE}
                  >
                    <Icon
                      name={icons.FOLDER}
                      size={17}
                    />

                    <span className={styles.path}>
                      {path}
                    </span>
                  </Label>

                  <Label
                    className={styles.detailsLabel}
                    size={sizes.LARGE}
                  >
                    <Icon
                      name={icons.DRIVE}
                      size={17}
                    />

                    <span className={styles.sizeOnDisk}>
                      {
                        formatBytes(sizeOnDisk)
                      }
                    </span>
                  </Label>

                  <Label
                    className={styles.detailsLabel}
                    title="Quality Profile"
                    size={sizes.LARGE}
                  >
                    <Icon
                      name={icons.PROFILE}
                      size={17}
                    />

                    <span className={styles.qualityProfileName}>
                      {
                        <QualityProfileNameConnector
                          qualityProfileId={qualityProfileId}
                        />
                      }
                    </span>
                  </Label>

                  <Label
                    className={styles.detailsLabel}
                    size={sizes.LARGE}
                  >
                    <Icon
                      name={monitored ? icons.MONITORED : icons.UNMONITORED}
                      size={17}
                    />

                    <span className={styles.qualityProfileName}>
                      {monitored ? 'Monitored' : 'Unmonitored'}
                    </span>
                  </Label>

                  {
                    !!studio &&
                      <Label
                        className={styles.detailsLabel}
                        title="Studio"
                        size={sizes.LARGE}
                      >
                        <Icon
                          name={icons.STUDIO}
                          size={17}
                        />

                        <span className={styles.qualityProfileName}>
                          {studio}
                        </span>
                      </Label>
                  }

                  <Tooltip
                    anchor={
                      <Label
                        className={styles.detailsLabel}
                        size={sizes.LARGE}
                      >
                        <Icon
                          name={icons.EXTERNAL_LINK}
                          size={17}
                        />

                        <span className={styles.links}>
                          Links
                        </span>
                      </Label>
                    }
                    tooltip={
                      <MovieDetailsLinks
                        tmdbId={tmdbId}
                        imdbId={imdbId}
                      />
                    }
                    kind={kinds.INVERSE}
                    position={tooltipPositions.BOTTOM}
                  />

                  {
                    !!tags.length &&
                      <Tooltip
                        anchor={
                          <Label
                            className={styles.detailsLabel}
                            size={sizes.LARGE}
                          >
                            <Icon
                              name={icons.TAGS}
                              size={17}
                            />

                            <span className={styles.tags}>
                              Tags
                            </span>
                          </Label>
                        }
                        tooltip={<MovieTagsConnector seriesId={id} />}
                        kind={kinds.INVERSE}
                        position={tooltipPositions.BOTTOM}
                      />

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
              !isPopulated && !movieFilesError &&
                <LoadingIndicator />
            }

            {
              !isFetching && movieFilesError &&
                <div>Loading movie files failed</div>
            }

          </div>

          <OrganizePreviewModalConnector
            isOpen={isOrganizeModalOpen}
            movieId={id}
            onModalClose={this.onOrganizeModalClose}
          />

          <MovieFileEditorModal
            isOpen={isManageEpisodesOpen}
            movieId={id}
            onModalClose={this.onManageEpisodesModalClose}
          />

          <MovieHistoryModal
            isOpen={isMovieHistoryModalOpen}
            movieId={id}
            onModalClose={this.onMovieHistoryModalClose}
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
            folder={path}
            allowSeriesChange={false}
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
  runtime: PropTypes.number.isRequired,
  ratings: PropTypes.object.isRequired,
  path: PropTypes.string.isRequired,
  sizeOnDisk: PropTypes.number.isRequired,
  qualityProfileId: PropTypes.number.isRequired,
  monitored: PropTypes.bool.isRequired,
  status: PropTypes.string.isRequired,
  studio: PropTypes.string,
  overview: PropTypes.string.isRequired,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  alternateTitles: PropTypes.arrayOf(PropTypes.string).isRequired,
  tags: PropTypes.arrayOf(PropTypes.number).isRequired,
  isSaving: PropTypes.bool.isRequired,
  isRefreshing: PropTypes.bool.isRequired,
  isSearching: PropTypes.bool.isRequired,
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  movieFilesError: PropTypes.object,
  hasMovieFiles: PropTypes.bool.isRequired,
  previousMovie: PropTypes.object.isRequired,
  nextMovie: PropTypes.object.isRequired,
  onMonitorTogglePress: PropTypes.func.isRequired,
  onRefreshPress: PropTypes.func.isRequired,
  onSearchPress: PropTypes.func.isRequired
};

MovieDetails.defaultProps = {
  tag: [],
  isSaving: false
};

export default MovieDetails;
