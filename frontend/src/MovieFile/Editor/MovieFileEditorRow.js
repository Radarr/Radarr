import PropTypes from 'prop-types';
import React, { Component } from 'react';
import IconButton from 'Components/Link/IconButton';
import { icons, kinds } from 'Helpers/Props';
import TableRow from 'Components/Table/TableRow';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableRowCellButton from 'Components/Table/Cells/TableRowCellButton';
import MovieQuality from 'Movie/MovieQuality';
import MovieLanguage from 'Movie/MovieLanguage';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import SelectQualityModal from 'MovieFile/Quality/SelectQualityModal';
import SelectLanguageModal from 'MovieFile/Language/SelectLanguageModal';
import * as mediaInfoTypes from 'MovieFile/mediaInfoTypes';
import MediaInfoConnector from 'MovieFile/MediaInfoConnector';
import MovieFileRowCellPlaceholder from './MovieFileRowCellPlaceholder';
import styles from './MovieFileEditorRow.css';

class MovieFileEditorRow extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isSelectQualityModalOpen: false,
      isSelectLanguageModalOpen: false,
      isConfirmDeleteModalOpen: false
    };
  }

  //
  // Listeners

  onSelectQualityPress = () => {
    this.setState({ isSelectQualityModalOpen: true });
  }

  onSelectLanguagePress = () => {
    this.setState({ isSelectLanguageModalOpen: true });
  }

  onSelectQualityModalClose = () => {
    this.setState({ isSelectQualityModalOpen: false });
  }

  onSelectLanguageModalClose = () => {
    this.setState({ isSelectLanguageModalOpen: false });
  }

  onDeletePress = () => {
    this.setState({ isConfirmDeleteModalOpen: true });
  }

  onConfirmDelete = () => {
    this.setState({ isConfirmDeleteModalOpen: false });

    this.props.onDeletePress(this.props.id);
  }

  onConfirmDeleteModalClose = () => {
    this.setState({ isConfirmDeleteModalOpen: false });
  }

  //
  // Render

  render() {
    const {
      id,
      relativePath,
      quality,
      qualityCutoffNotMet,
      languages
    } = this.props;

    const {
      isSelectQualityModalOpen,
      isSelectLanguageModalOpen,
      isConfirmDeleteModalOpen
    } = this.state;

    const showQualityPlaceholder = !quality;

    const showLanguagePlaceholder = !languages;

    return (
      <TableRow>
        <TableRowCell
          className={styles.relativePath}
          title={relativePath}
        >
          {relativePath}
        </TableRowCell>

        <TableRowCell>
          <MediaInfoConnector
            movieFileId={id}
            type={mediaInfoTypes.VIDEO}
          />
          <MediaInfoConnector
            movieFileId={id}
            type={mediaInfoTypes.AUDIO}
          />
        </TableRowCell>

        <TableRowCellButton
          className={styles.language}
          title="Click to change language"
          onPress={this.onSelectLanguagePress}
        >
          {
            showLanguagePlaceholder &&
              <MovieFileRowCellPlaceholder />
          }

          {
            !showLanguagePlaceholder && !!languages &&
              <MovieLanguage
                className={styles.label}
                languages={languages}
              />
          }
        </TableRowCellButton>

        <TableRowCellButton
          className={styles.quality}
          title="Click to change quality"
          onPress={this.onSelectQualityPress}
        >
          {
            showQualityPlaceholder &&
              <MovieFileRowCellPlaceholder />
          }

          {
            !showQualityPlaceholder && !!quality &&
              <MovieQuality
                className={styles.label}
                quality={quality}
                isCutoffNotMet={qualityCutoffNotMet}
              />
          }
        </TableRowCellButton>

        <TableRowCell className={styles.actions}>
          <IconButton
            title="Delete file"
            name={icons.REMOVE}
            onPress={this.onDeletePress}
          />
        </TableRowCell>

        <ConfirmModal
          isOpen={isConfirmDeleteModalOpen}
          ids={[id]}
          kind={kinds.DANGER}
          title="Delete Selected Movie Files"
          message={'Are you sure you want to delete the selected movie files?'}
          confirmLabel="Delete"
          onConfirm={this.onConfirmDelete}
          onCancel={this.onConfirmDeleteModalClose}
        />

        <SelectQualityModal
          isOpen={isSelectQualityModalOpen}
          ids={[id]}
          qualityId={quality ? quality.quality.id : 0}
          proper={quality ? quality.revision.version > 1 : false}
          real={quality ? quality.revision.real > 0 : false}
          onModalClose={this.onSelectQualityModalClose}
        />

        <SelectLanguageModal
          isOpen={isSelectLanguageModalOpen}
          ids={[id]}
          languageId={languages[0] ? languages[0].id : 0}
          onModalClose={this.onSelectLanguageModalClose}
        />
      </TableRow>
    );
  }

}

MovieFileEditorRow.propTypes = {
  id: PropTypes.number.isRequired,
  size: PropTypes.number.isRequired,
  relativePath: PropTypes.string.isRequired,
  quality: PropTypes.object.isRequired,
  qualityCutoffNotMet: PropTypes.bool.isRequired,
  languages: PropTypes.arrayOf(PropTypes.object).isRequired,
  mediaInfo: PropTypes.object.isRequired,
  onDeletePress: PropTypes.func.isRequired
};

export default MovieFileEditorRow;
