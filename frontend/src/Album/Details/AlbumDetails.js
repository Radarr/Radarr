import _ from 'lodash';
import moment from 'moment';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import formatBytes from 'Utilities/Number/formatBytes';
import selectAll from 'Utilities/Table/selectAll';
import toggleSelected from 'Utilities/Table/toggleSelected';
import { align, icons, sizes } from 'Helpers/Props';
import HeartRating from 'Components/HeartRating';
import Icon from 'Components/Icon';
import IconButton from 'Components/Link/IconButton';
import Label from 'Components/Label';
import AlbumCover from 'Album/AlbumCover';
import OrganizePreviewModalConnector from 'Organize/OrganizePreviewModalConnector';
import EditAlbumModalConnector from 'Album/Edit/EditAlbumModalConnector';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBodyConnector from 'Components/Page/PageContentBodyConnector';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import AlbumDetailsMediumConnector from './AlbumDetailsMediumConnector';
import ArtistHistoryModal from 'Artist/History/ArtistHistoryModal';
import InteractiveAlbumSearchModal from 'Album/Search/InteractiveAlbumSearchModal';
import TrackFileEditorModal from 'TrackFile/Editor/TrackFileEditorModal';

import styles from './AlbumDetails.css';

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

class AlbumDetails extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isOrganizeModalOpen: false,
      isArtistHistoryModalOpen: false,
      isInteractiveSearchModalOpen: false,
      isManageTracksOpen: false,
      isEditAlbumModalOpen: false,
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

  onEditAlbumPress = () => {
    this.setState({ isEditAlbumModalOpen: true });
  }

  onEditAlbumModalClose = () => {
    this.setState({ isEditAlbumModalOpen: false });
  }

  onManageTracksPress = () => {
    this.setState({ isManageTracksOpen: true });
  }

  onManageTracksModalClose = () => {
    this.setState({ isManageTracksOpen: false });
  }

  onInteractiveSearchPress = () => {
    this.setState({ isInteractiveSearchModalOpen: true });
  }

  onInteractiveSearchModalClose = () => {
    this.setState({ isInteractiveSearchModalOpen: false });
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
      title,
      disambiguation,
      albumType,
      statistics,
      monitored,
      releaseDate,
      ratings,
      images,
      media,
      isFetching,
      isPopulated,
      albumsError,
      trackFilesError,
      shortDateFormat,
      artist,
      previousAlbum,
      nextAlbum,
      isSearching,
      onSearchPress
    } = this.props;

    const {
      isOrganizeModalOpen,
      isArtistHistoryModalOpen,
      isInteractiveSearchModalOpen,
      isEditAlbumModalOpen,
      isManageTracksOpen,
      allExpanded,
      allCollapsed,
      expandedState
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

            <PageToolbarButton
              label="Interactive Search"
              iconName={icons.INTERACTIVE}
              onPress={this.onInteractiveSearchPress}
            />

            <PageToolbarSeparator />

            <PageToolbarButton
              label="Preview Rename"
              iconName={icons.ORGANIZE}
              onPress={this.onOrganizePress}
            />

            <PageToolbarButton
              label="Manage Tracks"
              iconName={icons.TRACK_FILE}
              onPress={this.onManageTracksPress}
            />

            <PageToolbarButton
              label="History"
              iconName={icons.HISTORY}
              onPress={this.onArtistHistoryPress}
            />

            <PageToolbarSeparator />

            <PageToolbarButton
              label="Edit"
              iconName={icons.EDIT}
              onPress={this.onEditAlbumPress}
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
                size={500}
                lazy={false}
              />

              <div className={styles.info}>
                <div className={styles.titleContainer}>
                  <div className={styles.title}>
                    {title}{disambiguation ? ` (${disambiguation})` : ''}
                  </div>

                  <div className={styles.artistNavigationButtons}>
                    <IconButton
                      className={styles.artistNavigationButton}
                      name={icons.ARROW_LEFT}
                      size={30}
                      title={`Go to ${previousAlbum.title}`}
                      to={`/album/${previousAlbum.foreignAlbumId}`}
                    />

                    <IconButton
                      className={styles.artistNavigationButton}
                      name={icons.ARROW_UP}
                      size={30}
                      title={`Go to ${artist.artistName}`}
                      to={`/artist/${artist.foreignArtistId}`}
                    />

                    <IconButton
                      className={styles.artistNavigationButton}
                      name={icons.ARROW_RIGHT}
                      size={30}
                      title={`Go to ${nextAlbum.title}`}
                      to={`/album/${nextAlbum.foreignAlbumId}`}
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

                  {
                    !!albumType &&
                      <Label
                        className={styles.detailsLabel}
                        title="Type"
                        size={sizes.LARGE}
                      >
                        <Icon
                          name={icons.INFO}
                          size={17}
                        />

                        <span className={styles.qualityProfileName}>
                          {albumType}
                        </span>
                      </Label>
                  }

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
              isPopulated && !!media.length &&
                <div>

                  {
                    media.slice(0).map((medium) => {
                      return (
                        <AlbumDetailsMediumConnector
                          key={medium.mediumNumber}
                          albumId={id}
                          albumMonitored={monitored}
                          {...medium}
                          isExpanded={expandedState[medium.mediumNumber]}
                          onExpandPress={this.onExpandPress}
                        />
                      );
                    })
                  }
                </div>
            }

          </div>

          <OrganizePreviewModalConnector
            isOpen={isOrganizeModalOpen}
            artistId={artist.id}
            albumId={id}
            onModalClose={this.onOrganizeModalClose}
          />

          <TrackFileEditorModal
            isOpen={isManageTracksOpen}
            artistId={artist.id}
            albumId={id}
            onModalClose={this.onManageTracksModalClose}
          />

          <InteractiveAlbumSearchModal
            isOpen={isInteractiveSearchModalOpen}
            albumId={id}
            onModalClose={this.onInteractiveSearchModalClose}
          />

          <ArtistHistoryModal
            isOpen={isArtistHistoryModalOpen}
            artistId={artist.id}
            albumId={id}
            onModalClose={this.onArtistHistoryModalClose}
          />

          <EditAlbumModalConnector
            isOpen={isEditAlbumModalOpen}
            albumId={id}
            artistId={artist.id}
            onModalClose={this.onEditAlbumModalClose}
          />

        </PageContentBodyConnector>
      </PageContent>
    );
  }
}

AlbumDetails.propTypes = {
  id: PropTypes.number.isRequired,
  foreignAlbumId: PropTypes.string.isRequired,
  title: PropTypes.string.isRequired,
  disambiguation: PropTypes.string,
  albumType: PropTypes.string.isRequired,
  statistics: PropTypes.object.isRequired,
  releaseDate: PropTypes.string.isRequired,
  ratings: PropTypes.object.isRequired,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  media: PropTypes.arrayOf(PropTypes.object).isRequired,
  monitored: PropTypes.bool.isRequired,
  shortDateFormat: PropTypes.string.isRequired,
  isSearching: PropTypes.bool,
  isFetching: PropTypes.bool,
  isPopulated: PropTypes.bool,
  albumsError: PropTypes.object,
  tracksError: PropTypes.object,
  trackFilesError: PropTypes.object,
  artist: PropTypes.object,
  previousAlbum: PropTypes.object,
  nextAlbum: PropTypes.object,
  onRefreshPress: PropTypes.func,
  onSearchPress: PropTypes.func.isRequired
};

AlbumDetails.defaultProps = {
  isSaving: false
};

export default AlbumDetails;
