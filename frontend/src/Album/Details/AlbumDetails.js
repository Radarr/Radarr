import _ from 'lodash';
import moment from 'moment';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { Tab, Tabs, TabList, TabPanel } from 'react-tabs';
import TextTruncate from 'react-text-truncate';
import formatBytes from 'Utilities/Number/formatBytes';
import selectAll from 'Utilities/Table/selectAll';
import toggleSelected from 'Utilities/Table/toggleSelected';
import { align, icons, kinds, sizes, tooltipPositions } from 'Helpers/Props';
import fonts from 'Styles/Variables/fonts';
import HeartRating from 'Components/HeartRating';
import Icon from 'Components/Icon';
import IconButton from 'Components/Link/IconButton';
import Label from 'Components/Label';
import MonitorToggleButton from 'Components/MonitorToggleButton';
import Tooltip from 'Components/Tooltip/Tooltip';
import AlbumCover from 'Album/AlbumCover';
import OrganizePreviewModalConnector from 'Organize/OrganizePreviewModalConnector';
// import RetagPreviewModalConnector from 'Retag/RetagPreviewModalConnector';
import DeleteAlbumModal from 'Album/Delete/DeleteAlbumModal';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBodyConnector from 'Components/Page/PageContentBodyConnector';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import ArtistHistoryTable from 'Artist/History/ArtistHistoryTable';
import InteractiveSearchTable from 'InteractiveSearch/InteractiveSearchTable';
import InteractiveSearchFilterMenuConnector from 'InteractiveSearch/InteractiveSearchFilterMenuConnector';
import TrackFileEditorTable from 'TrackFile/Editor/TrackFileEditorTable';
import AlbumDetailsLinks from './AlbumDetailsLinks';
import styles from './AlbumDetails.css';

const defaultFontSize = parseInt(fonts.defaultFontSize);
const lineHeight = parseFloat(fonts.lineHeight);

function getFanartUrl(images) {
  const fanartImage = _.find(images, { coverType: 'fanart' });
  if (fanartImage) {
    // Remove protocol
    return fanartImage.url.replace(/^https?:/, '');
  }
}

function formatDuration(timeSpan) {
  const duration = moment.duration(timeSpan);
  const hours = duration.get('hours');
  const minutes = duration.get('minutes');
  let hoursText = 'Hours';
  let minText = 'Minutes';

  if (minutes === 1) {
    minText = 'Minute';
  }

  if (hours === 0) {
    return `${minutes} ${minText}`;
  }

  if (hours === 1) {
    hoursText = 'Hour';
  }

  return `${hours} ${hoursText} ${minutes} ${minText}`;
}

function getExpandedState(newState) {
  return {
    allExpanded: newState.allSelected,
    allCollapsed: newState.allUnselected,
    expandedState: newState.selectedState
  };
}

class AlbumDetails extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isOrganizeModalOpen: false,
      isRetagModalOpen: false,
      isDeleteAlbumModalOpen: false,
      allExpanded: false,
      allCollapsed: false,
      expandedState: {},
      selectedTabIndex: 0
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

  onRetagPress = () => {
    this.setState({ isRetagModalOpen: true });
  }

  onRetagModalClose = () => {
    this.setState({ isRetagModalOpen: false });
  }

  onDeleteAlbumPress = () => {
    this.setState({
      isDeleteAlbumModalOpen: true
    });
  }

  onDeleteAlbumModalClose = () => {
    this.setState({ isDeleteAlbumModalOpen: false });
  }

  onExpandAllPress = () => {
    const {
      allExpanded,
      expandedState
    } = this.state;

    this.setState(getExpandedState(selectAll(expandedState, !allExpanded)));
  }

  onExpandPress = (bookId, isExpanded) => {
    this.setState((state) => {
      const convertedState = {
        allSelected: state.allExpanded,
        allUnselected: state.allCollapsed,
        selectedState: state.expandedState
      };

      const newState = toggleSelected(convertedState, [], bookId, isExpanded, false);

      return getExpandedState(newState);
    });
  }

  //
  // Render

  render() {
    const {
      id,
      titleSlug,
      title,
      disambiguation,
      duration,
      overview,
      statistics = {},
      monitored,
      releaseDate,
      ratings,
      images,
      links,
      isSaving,
      isFetching,
      isPopulated,
      trackFilesError,
      hasTrackFiles,
      shortDateFormat,
      artist,
      previousAlbum,
      nextAlbum,
      isSearching,
      onMonitorTogglePress,
      onSearchPress
    } = this.props;

    const {
      isOrganizeModalOpen,
      // isRetagModalOpen,
      isDeleteAlbumModalOpen,
      allExpanded,
      allCollapsed,
      selectedTabIndex
    } = this.state;

    let expandIcon = icons.EXPAND_INDETERMINATE;

    if (allExpanded) {
      expandIcon = icons.COLLAPSE;
    } else if (allCollapsed) {
      expandIcon = icons.EXPAND;
    }

    return (
      <PageContent title={title}>
        <PageToolbar>
          <PageToolbarSection>
            <PageToolbarButton
              label="Search Album"
              iconName={icons.SEARCH}
              isSpinning={isSearching}
              onPress={onSearchPress}
            />

            <PageToolbarSeparator />

            <PageToolbarButton
              label="Preview Rename"
              iconName={icons.ORGANIZE}
              isDisabled={!hasTrackFiles}
              onPress={this.onOrganizePress}
            />

            <PageToolbarButton
              label="Preview Retag"
              iconName={icons.RETAG}
              isDisabled={!hasTrackFiles}
              onPress={this.onRetagPress}
            />

            <PageToolbarSeparator />

            <PageToolbarButton
              label="Delete"
              iconName={icons.DELETE}
              onPress={this.onDeleteAlbumPress}
            />

          </PageToolbarSection>
          <PageToolbarSection alignContent={align.RIGHT}>
            <PageToolbarButton
              label={allExpanded ? 'Collapse All' : 'Expand All'}
              iconName={expandIcon}
              onPress={this.onExpandAllPress}
            />
          </PageToolbarSection>
        </PageToolbar>

        <PageContentBodyConnector innerClassName={styles.innerContentBody}>
          <div className={styles.header}>
            <div
              className={styles.backdrop}
              style={{
                backgroundImage: `url(${getFanartUrl(artist.images)})`
              }}
            >
              <div className={styles.backdropOverlay} />
            </div>

            <div className={styles.headerContent}>
              <AlbumCover
                className={styles.cover}
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
                      {title}{disambiguation ? ` (${disambiguation})` : ''}
                    </div>
                  </div>

                  <div className={styles.albumNavigationButtons}>
                    <IconButton
                      className={styles.albumNavigationButton}
                      name={icons.ARROW_LEFT}
                      size={30}
                      title={`Go to ${previousAlbum.title}`}
                      to={`/book/${previousAlbum.titleSlug}`}
                    />

                    <IconButton
                      className={styles.albumNavigationButton}
                      name={icons.ARROW_UP}
                      size={30}
                      title={`Go to ${artist.artistName}`}
                      to={`/author/${artist.titleSlug}`}
                    />

                    <IconButton
                      className={styles.albumNavigationButton}
                      name={icons.ARROW_RIGHT}
                      size={30}
                      title={`Go to ${nextAlbum.title}`}
                      to={`/book/${nextAlbum.titleSlug}`}
                    />
                  </div>
                </div>

                <div className={styles.details}>
                  <div>
                    {
                      !!duration &&
                        <span className={styles.duration}>
                          {formatDuration(duration)}
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
                      name={icons.CALENDAR}
                      size={17}
                    />

                    <span className={styles.sizeOnDisk}>
                      {
                        moment(releaseDate).format(shortDateFormat)
                      }
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
                        formatBytes(statistics.sizeOnDisk)
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
                      <AlbumDetailsLinks
                        titleSlug={titleSlug}
                        links={links}
                      />
                    }
                    kind={kinds.INVERSE}
                    position={tooltipPositions.BOTTOM}
                  />

                </div>
                <div className={styles.overview}>
                  <TextTruncate
                    line={Math.floor(125 / (defaultFontSize * lineHeight))}
                    text={overview.replace(/<[^>]*>?/gm, '')}
                  />
                </div>
              </div>
            </div>
          </div>

          <div className={styles.contentContainer}>
            {
              !isPopulated && !trackFilesError &&
                <LoadingIndicator />
            }

            {
              !isFetching && trackFilesError &&
                <div>Loading book files failed</div>
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

                {
                  selectedTabIndex === 1 &&
                    <div className={styles.filterIcon}>
                      <InteractiveSearchFilterMenuConnector
                        type="album"
                      />
                    </div>
                }

              </TabList>

              <TabPanel>
                <ArtistHistoryTable
                  authorId={artist.id}
                  bookId={id}
                />
              </TabPanel>

              <TabPanel>
                <InteractiveSearchTable
                  bookId={id}
                  type="album"
                />
              </TabPanel>

              <TabPanel>
                <TrackFileEditorTable
                  authorId={artist.id}
                  bookId={id}
                />
              </TabPanel>
            </Tabs>
          </div>

          <OrganizePreviewModalConnector
            isOpen={isOrganizeModalOpen}
            authorId={artist.id}
            bookId={id}
            onModalClose={this.onOrganizeModalClose}
          />

          {/* <RetagPreviewModalConnector */}
          {/*   isOpen={isRetagModalOpen} */}
          {/*   authorId={artist.id} */}
          {/*   bookId={id} */}
          {/*   onModalClose={this.onRetagModalClose} */}
          {/* /> */}

          <DeleteAlbumModal
            isOpen={isDeleteAlbumModalOpen}
            bookId={id}
            titleSlug={artist.titleSlug}
            onModalClose={this.onDeleteAlbumModalClose}
          />

        </PageContentBodyConnector>
      </PageContent>
    );
  }
}

AlbumDetails.propTypes = {
  id: PropTypes.number.isRequired,
  titleSlug: PropTypes.string.isRequired,
  title: PropTypes.string.isRequired,
  disambiguation: PropTypes.string,
  duration: PropTypes.number,
  overview: PropTypes.string,
  statistics: PropTypes.object.isRequired,
  releaseDate: PropTypes.string.isRequired,
  ratings: PropTypes.object.isRequired,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  links: PropTypes.arrayOf(PropTypes.object).isRequired,
  monitored: PropTypes.bool.isRequired,
  shortDateFormat: PropTypes.string.isRequired,
  isSaving: PropTypes.bool.isRequired,
  isSearching: PropTypes.bool,
  isFetching: PropTypes.bool,
  isPopulated: PropTypes.bool,
  trackFilesError: PropTypes.object,
  hasTrackFiles: PropTypes.bool.isRequired,
  artist: PropTypes.object,
  previousAlbum: PropTypes.object,
  nextAlbum: PropTypes.object,
  onMonitorTogglePress: PropTypes.func.isRequired,
  onRefreshPress: PropTypes.func,
  onSearchPress: PropTypes.func.isRequired
};

AlbumDetails.defaultProps = {
  isSaving: false
};

export default AlbumDetails;
