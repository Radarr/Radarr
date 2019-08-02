import PropTypes from 'prop-types';
import React, { Component } from 'react';
import classNames from 'classnames';
import getProgressBarKind from 'Utilities/Artist/getProgressBarKind';
import formatBytes from 'Utilities/Number/formatBytes';
import { icons } from 'Helpers/Props';
import HeartRating from 'Components/HeartRating';
import IconButton from 'Components/Link/IconButton';
import Link from 'Components/Link/Link';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import ProgressBar from 'Components/ProgressBar';
import TagListConnector from 'Components/TagListConnector';
// import CheckInput from 'Components/Form/CheckInput';
import VirtualTableRow from 'Components/Table/VirtualTableRow';
import VirtualTableRowCell from 'Components/Table/Cells/VirtualTableRowCell';
import RelativeDateCellConnector from 'Components/Table/Cells/RelativeDateCellConnector';
import ArtistNameLink from 'Artist/ArtistNameLink';
import AlbumTitleLink from 'Album/AlbumTitleLink';
import EditArtistModalConnector from 'Artist/Edit/EditArtistModalConnector';
import DeleteArtistModal from 'Artist/Delete/DeleteArtistModal';
import ArtistBanner from 'Artist/ArtistBanner';
import hasGrowableColumns from './hasGrowableColumns';
import ArtistStatusCell from './ArtistStatusCell';
import styles from './ArtistIndexRow.css';

class ArtistIndexRow extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      hasBannerError: false,
      isEditArtistModalOpen: false,
      isDeleteArtistModalOpen: false
    };
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

  onUseSceneNumberingChange = () => {
    // Mock handler to satisfy `onChange` being required for `CheckInput`.
    //
  }

  onBannerLoad = () => {
    if (this.state.hasBannerError) {
      this.setState({ hasBannerError: false });
    }
  }

  onBannerLoadError = () => {
    if (!this.state.hasBannerError) {
      this.setState({ hasBannerError: true });
    }
  }

  //
  // Render

  render() {
    const {
      style,
      id,
      monitored,
      status,
      artistName,
      foreignArtistId,
      artistType,
      qualityProfile,
      metadataProfile,
      nextAlbum,
      lastAlbum,
      added,
      statistics,
      genres,
      ratings,
      path,
      tags,
      images,
      showBanners,
      showSearchAction,
      columns,
      isRefreshingArtist,
      isSearchingArtist,
      onRefreshArtistPress,
      onSearchPress
    } = this.props;

    const {
      albumCount,
      trackCount,
      trackFileCount,
      totalTrackCount,
      sizeOnDisk
    } = statistics;

    const {
      hasBannerError,
      isEditArtistModalOpen,
      isDeleteArtistModalOpen
    } = this.state;

    return (
      <VirtualTableRow style={style}>
        {
          columns.map((column) => {
            const {
              name,
              isVisible
            } = column;

            if (!isVisible) {
              return null;
            }

            if (name === 'status') {
              return (
                <ArtistStatusCell
                  key={name}
                  className={styles[name]}
                  artistType={artistType}
                  monitored={monitored}
                  status={status}
                  component={VirtualTableRowCell}
                />
              );
            }

            if (name === 'sortName') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={classNames(
                    styles[name],
                    showBanners && styles.banner,
                    showBanners && !hasGrowableColumns(columns) && styles.bannerGrow
                  )}
                >
                  {
                    showBanners ?
                      <Link
                        className={styles.link}
                        to={`/artist/${foreignArtistId}`}
                      >
                        <ArtistBanner
                          className={styles.bannerImage}
                          images={images}
                          lazy={false}
                          overflow={true}
                          onError={this.onBannerLoadError}
                          onLoad={this.onBannerLoad}
                        />

                        {
                          hasBannerError &&
                            <div className={styles.overlayTitle}>
                              {artistName}
                            </div>
                        }
                      </Link> :

                      <ArtistNameLink
                        foreignArtistId={foreignArtistId}
                        artistName={artistName}
                      />
                  }
                </VirtualTableRowCell>
              );
            }

            if (name === 'artistType') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  {artistType}
                </VirtualTableRowCell>
              );
            }

            if (name === 'qualityProfileId') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  {qualityProfile.name}
                </VirtualTableRowCell>
              );
            }

            if (name === 'metadataProfileId') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  {metadataProfile.name}
                </VirtualTableRowCell>
              );
            }

            if (name === 'nextAlbum') {
              if (nextAlbum) {
                return (
                  <VirtualTableRowCell
                    key={name}
                    className={styles[name]}
                  >
                    <AlbumTitleLink
                      title={nextAlbum.title}
                      disambiguation={nextAlbum.disambiguation}
                      foreignAlbumId={nextAlbum.foreignAlbumId}
                    />
                  </VirtualTableRowCell>
                );
              }
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  None
                </VirtualTableRowCell>
              );
            }

            if (name === 'lastAlbum') {
              if (lastAlbum) {
                return (
                  <VirtualTableRowCell
                    key={name}
                    className={styles[name]}
                  >
                    <AlbumTitleLink
                      title={lastAlbum.title}
                      disambiguation={lastAlbum.disambiguation}
                      foreignAlbumId={lastAlbum.foreignAlbumId}
                    />
                  </VirtualTableRowCell>
                );
              }
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  None
                </VirtualTableRowCell>
              );
            }

            if (name === 'added') {
              return (
                <RelativeDateCellConnector
                  key={name}
                  className={styles[name]}
                  date={added}
                  component={VirtualTableRowCell}
                />
              );
            }

            if (name === 'albumCount') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  {albumCount}
                </VirtualTableRowCell>
              );
            }

            if (name === 'trackProgress') {
              const progress = trackCount ? trackFileCount / trackCount * 100 : 100;

              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  <ProgressBar
                    progress={progress}
                    kind={getProgressBarKind(status, monitored, progress)}
                    showText={true}
                    text={`${trackFileCount} / ${trackCount}`}
                    title={`${trackFileCount} / ${trackCount} (Total: ${totalTrackCount})`}
                    width={125}
                  />
                </VirtualTableRowCell>
              );
            }

            if (name === 'trackCount') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  {totalTrackCount}
                </VirtualTableRowCell>
              );
            }

            if (name === 'path') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  {path}
                </VirtualTableRowCell>
              );
            }

            if (name === 'sizeOnDisk') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  {formatBytes(sizeOnDisk)}
                </VirtualTableRowCell>
              );
            }

            if (name === 'genres') {
              const joinedGenres = genres.join(', ');

              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  <span title={joinedGenres}>
                    {joinedGenres}
                  </span>
                </VirtualTableRowCell>
              );
            }

            if (name === 'ratings') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  <HeartRating
                    rating={ratings.value}
                  />
                </VirtualTableRowCell>
              );
            }

            if (name === 'tags') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  <TagListConnector
                    tags={tags}
                  />
                </VirtualTableRowCell>
              );
            }

            if (name === 'actions') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  <SpinnerIconButton
                    name={icons.REFRESH}
                    title="Refresh Artist"
                    isSpinning={isRefreshingArtist}
                    onPress={onRefreshArtistPress}
                  />

                  {
                    showSearchAction &&
                      <SpinnerIconButton
                        className={styles.action}
                        name={icons.SEARCH}
                        title="Search for monitored albums"
                        isSpinning={isSearchingArtist}
                        onPress={onSearchPress}
                      />
                  }

                  <IconButton
                    name={icons.EDIT}
                    title="Edit Artist"
                    onPress={this.onEditArtistPress}
                  />
                </VirtualTableRowCell>
              );
            }

            return null;
          })
        }

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
      </VirtualTableRow>
    );
  }
}

ArtistIndexRow.propTypes = {
  style: PropTypes.object.isRequired,
  id: PropTypes.number.isRequired,
  monitored: PropTypes.bool.isRequired,
  status: PropTypes.string.isRequired,
  artistName: PropTypes.string.isRequired,
  foreignArtistId: PropTypes.string.isRequired,
  artistType: PropTypes.string,
  qualityProfile: PropTypes.object.isRequired,
  metadataProfile: PropTypes.object.isRequired,
  nextAlbum: PropTypes.object,
  lastAlbum: PropTypes.object,
  added: PropTypes.string,
  statistics: PropTypes.object.isRequired,
  latestAlbum: PropTypes.object,
  path: PropTypes.string.isRequired,
  genres: PropTypes.arrayOf(PropTypes.string).isRequired,
  ratings: PropTypes.object.isRequired,
  tags: PropTypes.arrayOf(PropTypes.number).isRequired,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  showBanners: PropTypes.bool.isRequired,
  showSearchAction: PropTypes.bool.isRequired,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  isRefreshingArtist: PropTypes.bool.isRequired,
  isSearchingArtist: PropTypes.bool.isRequired,
  onRefreshArtistPress: PropTypes.func.isRequired,
  onSearchPress: PropTypes.func.isRequired
};

ArtistIndexRow.defaultProps = {
  statistics: {
    albumCount: 0,
    trackCount: 0,
    trackFileCount: 0,
    totalTrackCount: 0
  },
  genres: [],
  tags: []
};

export default ArtistIndexRow;
