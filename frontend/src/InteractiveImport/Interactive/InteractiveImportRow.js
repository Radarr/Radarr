import PropTypes from 'prop-types';
import React, { Component } from 'react';
import formatBytes from 'Utilities/Number/formatBytes';
import hasDifferentItems from 'Utilities/Object/hasDifferentItems';
import { icons, kinds, tooltipPositions } from 'Helpers/Props';
import Icon from 'Components/Icon';
import TableRow from 'Components/Table/TableRow';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableRowCellButton from 'Components/Table/Cells/TableRowCellButton';
import TableSelectCell from 'Components/Table/Cells/TableSelectCell';
import Popover from 'Components/Tooltip/Popover';
import EpisodeQuality from 'Album/EpisodeQuality';
import EpisodeLanguage from 'Album/EpisodeLanguage';
import SelectArtistModal from 'InteractiveImport/Artist/SelectArtistModal';
import SelectAlbumModal from 'InteractiveImport/Album/SelectAlbumModal';
import SelectTrackModal from 'InteractiveImport/Track/SelectTrackModal';
import SelectQualityModal from 'InteractiveImport/Quality/SelectQualityModal';
import SelectLanguageModal from 'InteractiveImport/Language/SelectLanguageModal';
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
      isSelectTrackModalOpen: false,
      isSelectQualityModalOpen: false,
      isSelectLanguageModalOpen: false
    };
  }

  componentDidMount() {
    const {
      id,
      artist,
      album,
      tracks,
      quality,
      language
    } = this.props;

    if (
      artist &&
      album != null &&
      tracks.length &&
      quality &&
      language
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
      language,
      isSelected,
      onValidRowChange
    } = this.props;

    if (
      prevProps.artist === artist &&
      prevProps.album === album &&
      !hasDifferentItems(prevProps.tracks, tracks) &&
      prevProps.quality === quality &&
      prevProps.language === language &&
      prevProps.isSelected === isSelected
    ) {
      return;
    }

    const isValid = !!(
      artist &&
      album &&
      tracks.length &&
      quality &&
      language
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

  onSelectLanguagePress = () => {
    this.setState({ isSelectLanguageModalOpen: true });
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

  onSelectLanguageModalClose = (changed) => {
    this.setState({ isSelectLanguageModalOpen: false });
    this.selectRowAfterChange(changed);
  }

  //
  // Render

  render() {
    const {
      id,
      relativePath,
      artist,
      album,
      tracks,
      quality,
      language,
      size,
      rejections,
      isSelected,
      onSelectedChange
    } = this.props;

    const {
      isSelectArtistModalOpen,
      isSelectAlbumModalOpen,
      isSelectTrackModalOpen,
      isSelectQualityModalOpen,
      isSelectLanguageModalOpen
    } = this.state;

    const artistName = artist ? artist.artistName : '';
    const albumTitle = album ? album.title : '';
    const trackNumbers = tracks.map((track) => `${track.mediumNumber}x${track.trackNumber}`)
      .join(', ');

    const showArtistPlaceholder = isSelected && !artist;
    const showAlbumNumberPlaceholder = isSelected && !!artist && !album;
    const showTrackNumbersPlaceholder = isSelected && !!album && !tracks.length;
    const showQualityPlaceholder = isSelected && !quality;
    const showLanguagePlaceholder = isSelected && !language;

    return (
      <TableRow>
        <TableSelectCell
          id={id}
          isSelected={isSelected}
          onSelectedChange={onSelectedChange}
        />

        <TableRowCell
          className={styles.relativePath}
          title={relativePath}
        >
          {relativePath}
        </TableRowCell>

        <TableRowCellButton
          onPress={this.onSelectArtistPress}
        >
          {
            showArtistPlaceholder ? <InteractiveImportRowCellPlaceholder /> : artistName
          }
        </TableRowCellButton>

        <TableRowCellButton
          isDisabled={!artist}
          onPress={this.onSelectAlbumPress}
        >
          {
            showAlbumNumberPlaceholder ? <InteractiveImportRowCellPlaceholder /> : albumTitle
          }
        </TableRowCellButton>

        <TableRowCellButton
          isDisabled={!artist || !album}
          onPress={this.onSelectTrackPress}
        >
          {
            showTrackNumbersPlaceholder ? <InteractiveImportRowCellPlaceholder /> : trackNumbers
          }
        </TableRowCellButton>

        <TableRowCellButton
          className={styles.quality}
          onPress={this.onSelectQualityPress}
        >
          {
            showQualityPlaceholder &&
              <InteractiveImportRowCellPlaceholder />
          }

          {
            !showQualityPlaceholder && !!quality &&
              <EpisodeQuality
                className={styles.label}
                quality={quality}
              />
          }
        </TableRowCellButton>

        <TableRowCellButton
          className={styles.language}
          onPress={this.onSelectLanguagePress}
        >
          {
            showLanguagePlaceholder &&
              <InteractiveImportRowCellPlaceholder />
          }

          {
            !showLanguagePlaceholder && !!language &&
              <EpisodeLanguage
                className={styles.label}
                language={language}
              />
          }
        </TableRowCellButton>

        <TableRowCell>
          {formatBytes(size)}
        </TableRowCell>

        <TableRowCell>
          {
            !!rejections.length &&
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
              />
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
          onModalClose={this.onSelectTrackModalClose}
        />

        <SelectQualityModal
          isOpen={isSelectQualityModalOpen}
          id={id}
          qualityId={quality ? quality.quality.id : 0}
          proper={quality ? quality.revision.version > 1 : false}
          real={quality ? quality.revision.real > 0 : false}
          onModalClose={this.onSelectQualityModalClose}
        />

        <SelectLanguageModal
          isOpen={isSelectLanguageModalOpen}
          id={id}
          languageId={language ? language.id : 0}
          onModalClose={this.onSelectLanguageModalClose}
        />
      </TableRow>
    );
  }

}

InteractiveImportRow.propTypes = {
  id: PropTypes.number.isRequired,
  relativePath: PropTypes.string.isRequired,
  artist: PropTypes.object,
  album: PropTypes.object,
  tracks: PropTypes.arrayOf(PropTypes.object).isRequired,
  quality: PropTypes.object,
  language: PropTypes.object,
  size: PropTypes.number.isRequired,
  rejections: PropTypes.arrayOf(PropTypes.object).isRequired,
  isSelected: PropTypes.bool,
  onSelectedChange: PropTypes.func.isRequired,
  onValidRowChange: PropTypes.func.isRequired
};

InteractiveImportRow.defaultProps = {
  tracks: []
};

export default InteractiveImportRow;
