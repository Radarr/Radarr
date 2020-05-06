import PropTypes from 'prop-types';
import React, { Component } from 'react';
import formatBytes from 'Utilities/Number/formatBytes';
import { icons, kinds, tooltipPositions } from 'Helpers/Props';
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
import SelectQualityModal from 'InteractiveImport/Quality/SelectQualityModal';
import InteractiveImportRowCellPlaceholder from './InteractiveImportRowCellPlaceholder';
import styles from './InteractiveImportRow.css';

class InteractiveImportRow extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isSelectArtistModalOpen: false,
      isSelectAlbumModalOpen: false,
      isSelectQualityModalOpen: false
    };
  }

  componentDidMount() {
    const {
      id,
      artist,
      album,
      quality
    } = this.props;

    if (
      artist &&
      album != null &&
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
      quality,
      isSelected,
      onValidRowChange
    } = this.props;

    if (
      prevProps.artist === artist &&
      prevProps.album === album &&
      prevProps.quality === quality &&
      prevProps.isSelected === isSelected
    ) {
      return;
    }

    const isValid = !!(
      artist &&
      album &&
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
      path,
      artist,
      album,
      quality,
      size,
      rejections,
      additionalFile,
      isSelected,
      onSelectedChange
    } = this.props;

    const {
      isSelectArtistModalOpen,
      isSelectAlbumModalOpen,
      isSelectQualityModalOpen
    } = this.state;

    const artistName = artist ? artist.artistName : '';
    let albumTitle = '';
    if (album) {
      albumTitle = album.disambiguation ? `${album.title} (${album.disambiguation})` : album.title;
    }

    const showArtistPlaceholder = isSelected && !artist;
    const showAlbumNumberPlaceholder = isSelected && !!artist && !album;
    const showQualityPlaceholder = isSelected && !quality;

    const pathCellContents = (
      <div>
        {path}
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
          className={styles.path}
          title={path}
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
          authorId={artist && artist.id}
          onModalClose={this.onSelectAlbumModalClose}
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
  path: PropTypes.string.isRequired,
  artist: PropTypes.object,
  album: PropTypes.object,
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

export default InteractiveImportRow;
