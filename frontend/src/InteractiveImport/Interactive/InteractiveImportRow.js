import PropTypes from 'prop-types';
import React, { Component } from 'react';
import formatBytes from 'Utilities/Number/formatBytes';
import hasDifferentItems from 'Utilities/Object/hasDifferentItems';
import { icons, kinds, tooltipPositions, sortDirections } from 'Helpers/Props';
import Icon from 'Components/Icon';
import TableRow from 'Components/Table/TableRow';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableRowCellButton from 'Components/Table/Cells/TableRowCellButton';
import TableSelectCell from 'Components/Table/Cells/TableSelectCell';
import Popover from 'Components/Tooltip/Popover';
import Tooltip from 'Components/Tooltip/Tooltip';
import TrackQuality from 'Album/TrackQuality';
import SelectArtistModal from 'InteractiveImport/Artist/SelectArtistModal';
import SelectAlbumModal from 'InteractiveImport/Album/SelectAlbumModal';
import SelectTrackModal from 'InteractiveImport/Track/SelectTrackModal';
import SelectQualityModal from 'InteractiveImport/Quality/SelectQualityModal';
import InteractiveImportRowCellPlaceholder from './InteractiveImportRowCellPlaceholder';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import styles from './InteractiveImportRow.css';

class InteractiveImportRow extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isSelectArtistModalOpen: false,
      isSelectAlbumModalOpen: false,
      isSelectTrackModalOpen: false,
      isSelectQualityModalOpen: false
    };
  }

  componentDidMount() {
    const {
      id,
      artist,
      album,
      tracks,
      quality
    } = this.props;

    if (
      artist &&
      album != null &&
      tracks.length &&
      quality
    ) {
      this.props.onSelectedChange({ id, value: true });
    }
  }

  componentDidUpdate(prevProps) {
    const {
      id,
      artist,
      album,
      tracks,
      quality,
      isSelected,
      onValidRowChange
    } = this.props;

    if (
      prevProps.artist === artist &&
      prevProps.album === album &&
      !hasDifferentItems(prevProps.tracks, tracks) &&
      prevProps.quality === quality &&
      prevProps.isSelected === isSelected
    ) {
      return;
    }

    const isValid = !!(
      artist &&
      album &&
      tracks.length &&
      quality
    );

    if (isSelected && !isValid) {
      onValidRowChange(id, false);
    } else {
      onValidRowChange(id, true);
    }
  }

  //
  // Control

  selectRowAfterChange = (value) => {
    const {
      id,
      isSelected
    } = this.props;

    if (!isSelected && value === true) {
      this.props.onSelectedChange({ id, value });
    }
  }

  //
  // Listeners

  onSelectArtistPress = () => {
    this.setState({ isSelectArtistModalOpen: true });
  }

  onSelectAlbumPress = () => {
    this.setState({ isSelectAlbumModalOpen: true });
  }

  onSelectTrackPress = () => {
    this.setState({ isSelectTrackModalOpen: true });
  }

  onSelectQualityPress = () => {
    this.setState({ isSelectQualityModalOpen: true });
  }

  onSelectArtistModalClose = (changed) => {
    this.setState({ isSelectArtistModalOpen: false });
    this.selectRowAfterChange(changed);
  }

  onSelectAlbumModalClose = (changed) => {
    this.setState({ isSelectAlbumModalOpen: false });
    this.selectRowAfterChange(changed);
  }

  onSelectTrackModalClose = (changed) => {
    this.setState({ isSelectTrackModalOpen: false });
    this.selectRowAfterChange(changed);
  }

  onSelectQualityModalClose = (changed) => {
    this.setState({ isSelectQualityModalOpen: false });
    this.selectRowAfterChange(changed);
  }

  //
  // Render

  render() {
    const {
      id,
      allowArtistChange,
      relativePath,
      artist,
      album,
      albumReleaseId,
      tracks,
      quality,
      size,
      rejections,
      audioTags,
      additionalFile,
      isSelected,
      isSaving,
      onSelectedChange
    } = this.props;

    const {
      isSelectArtistModalOpen,
      isSelectAlbumModalOpen,
      isSelectTrackModalOpen,
      isSelectQualityModalOpen
    } = this.state;

    const artistName = artist ? artist.artistName : '';
    let albumTitle = '';
    if (album) {
      albumTitle = album.disambiguation ? `${album.title} (${album.disambiguation})` : album.title;
    }

    const sortedTracks = tracks.sort((a, b) => parseInt(a.absoluteTrackNumber) - parseInt(b.absoluteTrackNumber));

    const trackNumbers = sortedTracks.map((track) => `${track.mediumNumber}x${track.trackNumber}`)
      .join(', ');

    const showArtistPlaceholder = isSelected && !artist;
    const showAlbumNumberPlaceholder = isSelected && !!artist && !album;
    const showTrackNumbersPlaceholder = !isSaving && isSelected && !!album && !tracks.length;
    const showTrackNumbersLoading = isSaving && !tracks.length;
    const showQualityPlaceholder = isSelected && !quality;

    const pathCellContents = (
      <div>
        {relativePath}
      </div>
    );

    const pathCell = additionalFile ? (
      <Tooltip
        anchor={pathCellContents}
        tooltip='This file is already in your library for a release you are currently importing'
        position={tooltipPositions.TOP}
      />
    ) : pathCellContents;

    return (
      <TableRow
        className={additionalFile ? styles.additionalFile : undefined}
      >
        <TableSelectCell
          id={id}
          isSelected={isSelected}
          onSelectedChange={onSelectedChange}
        />

        <TableRowCell
          className={styles.relativePath}
          title={relativePath}
        >
          {pathCell}
        </TableRowCell>

        <TableRowCellButton
          isDisabled={!allowArtistChange}
          title={allowArtistChange ? 'Click to change artist' : undefined}
          onPress={this.onSelectArtistPress}
        >
          {
            showArtistPlaceholder ? <InteractiveImportRowCellPlaceholder /> : artistName
          }
        </TableRowCellButton>

        <TableRowCellButton
          isDisabled={!artist}
          title={artist ? 'Click to change album' : undefined}
          onPress={this.onSelectAlbumPress}
        >
          {
            showAlbumNumberPlaceholder ? <InteractiveImportRowCellPlaceholder /> : albumTitle
          }
        </TableRowCellButton>

        <TableRowCellButton
          isDisabled={!artist || !album}
          title={artist && album ? 'Click to change track' : undefined}
          onPress={this.onSelectTrackPress}
        >
          {
            showTrackNumbersLoading && <LoadingIndicator size={20} className={styles.loading} />
          }
          {
            showTrackNumbersPlaceholder ? <InteractiveImportRowCellPlaceholder /> : trackNumbers
          }
        </TableRowCellButton>

        <TableRowCellButton
          className={styles.quality}
          title="Click to change quality"
          onPress={this.onSelectQualityPress}
        >
          {
            showQualityPlaceholder &&
              <InteractiveImportRowCellPlaceholder />
          }

          {
            !showQualityPlaceholder && !!quality &&
              <TrackQuality
                className={styles.label}
                quality={quality}
              />
          }
        </TableRowCellButton>

        <TableRowCell>
          {formatBytes(size)}
        </TableRowCell>

        <TableRowCell>
          {
            rejections && rejections.length ?
              <Popover
                anchor={
                  <Icon
                    name={icons.DANGER}
                    kind={kinds.DANGER}
                  />
                }
                title="Release Rejected"
                body={
                  <ul>
                    {
                      rejections.map((rejection, index) => {
                        return (
                          <li key={index}>
                            {rejection.reason}
                          </li>
                        );
                      })
                    }
                  </ul>
                }
                position={tooltipPositions.LEFT}
              /> :
              null
          }
        </TableRowCell>

        <SelectArtistModal
          isOpen={isSelectArtistModalOpen}
          ids={[id]}
          onModalClose={this.onSelectArtistModalClose}
        />

        <SelectAlbumModal
          isOpen={isSelectAlbumModalOpen}
          ids={[id]}
          artistId={artist && artist.id}
          onModalClose={this.onSelectAlbumModalClose}
        />

        <SelectTrackModal
          isOpen={isSelectTrackModalOpen}
          id={id}
          artistId={artist && artist.id}
          albumId={album && album.id}
          albumReleaseId={albumReleaseId}
          rejections={rejections}
          audioTags={audioTags}
          sortKey='mediumNumber'
          sortDirection={sortDirections.ASCENDING}
          filename={relativePath}
          onModalClose={this.onSelectTrackModalClose}
        />

        <SelectQualityModal
          isOpen={isSelectQualityModalOpen}
          ids={[id]}
          qualityId={quality ? quality.quality.id : 0}
          proper={quality ? quality.revision.version > 1 : false}
          real={quality ? quality.revision.real > 0 : false}
          onModalClose={this.onSelectQualityModalClose}
        />
      </TableRow>
    );
  }

}

InteractiveImportRow.propTypes = {
  id: PropTypes.number.isRequired,
  allowArtistChange: PropTypes.bool.isRequired,
  relativePath: PropTypes.string.isRequired,
  artist: PropTypes.object,
  album: PropTypes.object,
  albumReleaseId: PropTypes.number,
  tracks: PropTypes.arrayOf(PropTypes.object).isRequired,
  quality: PropTypes.object,
  size: PropTypes.number.isRequired,
  rejections: PropTypes.arrayOf(PropTypes.object).isRequired,
  audioTags: PropTypes.object.isRequired,
  additionalFile: PropTypes.bool.isRequired,
  isSelected: PropTypes.bool,
  isSaving: PropTypes.bool.isRequired,
  onSelectedChange: PropTypes.func.isRequired,
  onValidRowChange: PropTypes.func.isRequired
};

InteractiveImportRow.defaultProps = {
  tracks: []
};

export default InteractiveImportRow;
