import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
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
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBodyConnector from 'Components/Page/PageContentBodyConnector';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import Popover from 'Components/Tooltip/Popover';
import Tooltip from 'Components/Tooltip/Tooltip';
import TrackFileEditorModal from 'TrackFile/Editor/TrackFileEditorModal';
import OrganizePreviewModalConnector from 'Organize/OrganizePreviewModalConnector';
import RetagPreviewModalConnector from 'Retag/RetagPreviewModalConnector';
import QualityProfileNameConnector from 'Settings/Profiles/Quality/QualityProfileNameConnector';
import ArtistPoster from 'Artist/ArtistPoster';
import EditArtistModalConnector from 'Artist/Edit/EditArtistModalConnector';
import DeleteArtistModal from 'Artist/Delete/DeleteArtistModal';
import ArtistHistoryModal from 'Artist/History/ArtistHistoryModal';
import ArtistAlternateTitles from './ArtistAlternateTitles';
import ArtistDetailsSeasonConnector from './ArtistDetailsSeasonConnector';
import ArtistTagsConnector from './ArtistTagsConnector';
import ArtistDetailsLinks from './ArtistDetailsLinks';
import styles from './ArtistDetails.css';
import InteractiveImportModal from '../../InteractiveImport/InteractiveImportModal';
import ArtistInteractiveSearchModalConnector from 'Artist/Search/ArtistInteractiveSearchModalConnector';
import Link from 'Components/Link/Link';

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

class ArtistDetails extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isOrganizeModalOpen: false,
      isRetagModalOpen: false,
      isManageTracksOpen: false,
      isEditArtistModalOpen: false,
      isDeleteArtistModalOpen: false,
      isArtistHistoryModalOpen: false,
      isInteractiveImportModalOpen: false,
      isInteractiveSearchModalOpen: false,
      allExpanded: false,
      allCollapsed: false,
      expandedState: {}
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

  onManageTracksPress = () => {
    this.setState({ isManageTracksOpen: true });
  }

  onManageTracksModalClose = () => {
    this.setState({ isManageTracksOpen: false });
  }

  onInteractiveImportPress = () => {
    this.setState({ isInteractiveImportModalOpen: true });
  }

  onInteractiveImportModalClose = () => {
    this.setState({ isInteractiveImportModalOpen: false });
  }

  onInteractiveSearchPress = () => {
    this.setState({ isInteractiveSearchModalOpen: true });
  }

  onInteractiveSearchModalClose = () => {
    this.setState({ isInteractiveSearchModalOpen: false });
  }

  onEditArtistPress = () => {
    this.setState({ isEditArtistModalOpen: true });
  }

  onEditArtistModalClose = () => {
    this.setState({ isEditArtistModalOpen: false });
  }

  onDeleteArtistPress = () => {
    this.setState({
      isEditArtistModalOpen: false,
      isDeleteArtistModalOpen: true
    });
  }

  onDeleteArtistModalClose = () => {
    this.setState({ isDeleteArtistModalOpen: false });
  }

  onArtistHistoryPress = () => {
    this.setState({ isArtistHistoryModalOpen: true });
  }

  onArtistHistoryModalClose = () => {
    this.setState({ isArtistHistoryModalOpen: false });
  }

  onExpandAllPress = () => {
    const {
      allExpanded,
      expandedState
    } = this.state;

    this.setState(getExpandedState(selectAll(expandedState, !allExpanded)));
  }

  onExpandPress = (albumId, isExpanded) => {
    this.setState((state) => {
      const convertedState = {
        allSelected: state.allExpanded,
        allUnselected: state.allCollapsed,
        selectedState: state.expandedState
      };

      const newState = toggleSelected(convertedState, [], albumId, isExpanded, false);

      return getExpandedState(newState);
    });
  }

  //
  // Render

  render() {
    const {
      id,
      foreignArtistId,
      artistName,
      ratings,
      path,
      statistics,
      qualityProfileId,
      monitored,
      albumTypes,
      status,
      overview,
      links,
      images,
      artistType,
      alternateTitles,
      tags,
      isSaving,
      isRefreshing,
      isSearching,
      isFetching,
      isPopulated,
      albumsError,
      trackFilesError,
      hasAlbums,
      hasMonitoredAlbums,
      hasTrackFiles,
      previousArtist,
      nextArtist,
      onMonitorTogglePress,
      onRefreshPress,
      onSearchPress
    } = this.props;

    const {
      trackFileCount,
      sizeOnDisk
    } = statistics;

    const {
      isOrganizeModalOpen,
      isRetagModalOpen,
      isManageTracksOpen,
      isEditArtistModalOpen,
      isDeleteArtistModalOpen,
      isArtistHistoryModalOpen,
      isInteractiveImportModalOpen,
      isInteractiveSearchModalOpen,
      allExpanded,
      allCollapsed,
      expandedState
    } = this.state;

    const continuing = status === 'continuing';
    const endedString = artistType === 'Person' ? 'Deceased' : 'Ended';

    let trackFilesCountMessage = 'No track files';

    if (trackFileCount === 1) {
      trackFilesCountMessage = '1 track file';
    } else if (trackFileCount > 1) {
      trackFilesCountMessage = `${trackFileCount} track files`;
    }

    let expandIcon = icons.EXPAND_INDETERMINATE;

    if (allExpanded) {
      expandIcon = icons.COLLAPSE;
    } else if (allCollapsed) {
      expandIcon = icons.EXPAND;
    }

    return (
      <PageContent title={artistName}>
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
              label="Search Monitored"
              iconName={icons.SEARCH}
              isDisabled={!monitored || !hasMonitoredAlbums || !hasAlbums}
              isSpinning={isSearching}
              title={hasMonitoredAlbums ? undefined : 'No monitored albums for this artist'}
              onPress={onSearchPress}
            />

            <PageToolbarButton
              label="Interactive Search"
              iconName={icons.INTERACTIVE}
              isDisabled={!monitored || !hasMonitoredAlbums || !hasAlbums}
              isSpinning={isSearching}
              title={hasMonitoredAlbums ? undefined : 'No monitored albums for this artist'}
              onPress={this.onInteractiveSearchPress}
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

            <PageToolbarButton
              label="Manage Tracks"
              iconName={icons.TRACK_FILE}
              isDisabled={!hasTrackFiles}
              onPress={this.onManageTracksPress}
            />

            <PageToolbarButton
              label="History"
              iconName={icons.HISTORY}
              isDisabled={!hasAlbums}
              onPress={this.onArtistHistoryPress}
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
              onPress={this.onEditArtistPress}
            />

            <PageToolbarButton
              label="Delete"
              iconName={icons.DELETE}
              onPress={this.onDeleteArtistPress}
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
                backgroundImage: `url(${getFanartUrl(images)})`
              }}
            >
              <div className={styles.backdropOverlay} />
            </div>

            <div className={styles.headerContent}>
              <ArtistPoster
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
                      {artistName}
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
                            body={<ArtistAlternateTitles alternateTitles={alternateTitles} />}
                            position={tooltipPositions.BOTTOM}
                          />
                        </div>
                    }
                  </div>

                  <div className={styles.artistNavigationButtons}>
                    <IconButton
                      className={styles.artistNavigationButton}
                      name={icons.ARROW_LEFT}
                      size={30}
                      title={`Go to ${previousArtist.artistName}`}
                      to={`/artist/${previousArtist.foreignArtistId}`}
                    />

                    <IconButton
                      className={styles.artistNavigationButton}
                      name={icons.ARROW_UP}
                      size={30}
                      title={'Go to artist listing'}
                      to={'/'}
                    />

                    <IconButton
                      className={styles.artistNavigationButton}
                      name={icons.ARROW_RIGHT}
                      size={30}
                      title={`Go to ${nextArtist.artistName}`}
                      to={`/artist/${nextArtist.foreignArtistId}`}
                    />
                  </div>
                </div>

                <div className={styles.details}>
                  <div>
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
                    title={trackFilesCountMessage}
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

                  <Label
                    className={styles.detailsLabel}
                    title={continuing ? 'More albums are expected' : 'No additional albums are expected'}
                    size={sizes.LARGE}
                  >
                    <Icon
                      name={continuing ? icons.ARTIST_CONTINUING : icons.ARTIST_ENDED}
                      size={17}
                    />

                    <span className={styles.qualityProfileName}>
                      {continuing ? 'Continuing' : endedString}
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
                      <ArtistDetailsLinks
                        foreignArtistId={foreignArtistId}
                        links={links}
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
                        tooltip={<ArtistTagsConnector artistId={id} />}
                        kind={kinds.INVERSE}
                        position={tooltipPositions.BOTTOM}
                      />

                  }
                </div>
                <div className={styles.overview}>
                  <TextTruncate
                    line={Math.floor(125 / (defaultFontSize * lineHeight))}
                    text={overview}
                  />
                </div>
              </div>
            </div>
          </div>

          <div className={styles.contentContainer}>
            {
              !isPopulated && !albumsError && !trackFilesError &&
                <LoadingIndicator />
            }

            {
              !isFetching && albumsError &&
                <div>Loading albums failed</div>
            }

            {
              !isFetching && trackFilesError &&
                <div>Loading track files failed</div>
            }

            {
              isPopulated && !!albumTypes.length &&
                <div>
                  {
                    albumTypes.slice(0).map((albumType) => {
                      return (
                        <ArtistDetailsSeasonConnector
                          key={albumType}
                          artistId={id}
                          name={albumType}
                          label={albumType}
                          {...albumType}
                          isExpanded={expandedState[albumType]}
                          onExpandPress={this.onExpandPress}
                        />
                      );
                    })
                  }
                </div>
            }

          </div>

          <div className={styles.metadataMessage}>
            Missing Albums, Singles, or Other Types? Modify or Create a New <Link to='/settings/profiles'> Metadata Profile</Link>!
          </div>

          <OrganizePreviewModalConnector
            isOpen={isOrganizeModalOpen}
            artistId={id}
            onModalClose={this.onOrganizeModalClose}
          />

          <RetagPreviewModalConnector
            isOpen={isRetagModalOpen}
            artistId={id}
            onModalClose={this.onRetagModalClose}
          />

          <TrackFileEditorModal
            isOpen={isManageTracksOpen}
            artistId={id}
            onModalClose={this.onManageTracksModalClose}
          />

          <ArtistHistoryModal
            isOpen={isArtistHistoryModalOpen}
            artistId={id}
            onModalClose={this.onArtistHistoryModalClose}
          />

          <EditArtistModalConnector
            isOpen={isEditArtistModalOpen}
            artistId={id}
            onModalClose={this.onEditArtistModalClose}
            onDeleteArtistPress={this.onDeleteArtistPress}
          />

          <DeleteArtistModal
            isOpen={isDeleteArtistModalOpen}
            artistId={id}
            onModalClose={this.onDeleteArtistModalClose}
          />

          <InteractiveImportModal
            isOpen={isInteractiveImportModalOpen}
            folder={path}
            allowArtistChange={false}
            showFilterExistingFiles={true}
            showImportMode={false}
            onModalClose={this.onInteractiveImportModalClose}
          />

          <ArtistInteractiveSearchModalConnector
            isOpen={isInteractiveSearchModalOpen}
            artistId={id}
            onModalClose={this.onInteractiveSearchModalClose}
          />
        </PageContentBodyConnector>
      </PageContent>
    );
  }
}

ArtistDetails.propTypes = {
  id: PropTypes.number.isRequired,
  foreignArtistId: PropTypes.string.isRequired,
  artistName: PropTypes.string.isRequired,
  ratings: PropTypes.object.isRequired,
  path: PropTypes.string.isRequired,
  statistics: PropTypes.object.isRequired,
  qualityProfileId: PropTypes.number.isRequired,
  monitored: PropTypes.bool.isRequired,
  artistType: PropTypes.string,
  albumTypes: PropTypes.arrayOf(PropTypes.string),
  status: PropTypes.string.isRequired,
  overview: PropTypes.string.isRequired,
  links: PropTypes.arrayOf(PropTypes.object).isRequired,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  alternateTitles: PropTypes.arrayOf(PropTypes.string).isRequired,
  tags: PropTypes.arrayOf(PropTypes.number).isRequired,
  isSaving: PropTypes.bool.isRequired,
  isRefreshing: PropTypes.bool.isRequired,
  isSearching: PropTypes.bool.isRequired,
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  albumsError: PropTypes.object,
  trackFilesError: PropTypes.object,
  hasAlbums: PropTypes.bool.isRequired,
  hasMonitoredAlbums: PropTypes.bool.isRequired,
  hasTrackFiles: PropTypes.bool.isRequired,
  previousArtist: PropTypes.object.isRequired,
  nextArtist: PropTypes.object.isRequired,
  onMonitorTogglePress: PropTypes.func.isRequired,
  onRefreshPress: PropTypes.func.isRequired,
  onSearchPress: PropTypes.func.isRequired
};

ArtistDetails.defaultProps = {
  statistics: {},
  tags: [],
  isSaving: false
};

export default ArtistDetails;
