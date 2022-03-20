import PropTypes from 'prop-types';
import React, { Component } from 'react';
import IconButton from 'Components/Link/IconButton';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableRow from 'Components/Table/TableRow';
import { icons, kinds } from 'Helpers/Props';
import MovieFormats from 'Movie/MovieFormats';
import MovieLanguage from 'Movie/MovieLanguage';
import MovieQuality from 'Movie/MovieQuality';
import FileEditModal from 'MovieFile/Edit/FileEditModal';
import MediaInfoConnector from 'MovieFile/MediaInfoConnector';
import * as mediaInfoTypes from 'MovieFile/mediaInfoTypes';
import formatBytes from 'Utilities/Number/formatBytes';
import translate from 'Utilities/String/translate';
import FileDetailsModal from '../FileDetailsModal';
import MovieFileRowCellPlaceholder from './MovieFileRowCellPlaceholder';
import styles from './MovieFileEditorRow.css';

class MovieFileEditorRow extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isConfirmDeleteModalOpen: false,
      isFileDetailsModalOpen: false,
      isFileEditModalOpen: false
    };
  }

  //
  // Listeners

  onDeletePress = () => {
    this.setState({ isConfirmDeleteModalOpen: true });
  };

  onConfirmDelete = () => {
    this.setState({ isConfirmDeleteModalOpen: false });

    this.props.onDeletePress(this.props.id);
  };

  onConfirmDeleteModalClose = () => {
    this.setState({ isConfirmDeleteModalOpen: false });
  };

  onFileDetailsPress = () => {
    this.setState({ isFileDetailsModalOpen: true });
  };

  onFileDetailsModalClose = () => {
    this.setState({ isFileDetailsModalOpen: false });
  };

  onFileEditPress = () => {
    this.setState({ isFileEditModalOpen: true });
  };

  onFileEditModalClose = () => {
    this.setState({ isFileEditModalOpen: false });
  };

  //
  // Render

  render() {
    const {
      id,
      mediaInfo,
      relativePath,
      size,
      releaseGroup,
      quality,
      qualityCutoffNotMet,
      customFormats,
      languages
    } = this.props;

    const {
      isFileDetailsModalOpen,
      isFileEditModalOpen,
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
        </TableRowCell>

        <TableRowCell>
          <MediaInfoConnector
            movieFileId={id}
            type={mediaInfoTypes.AUDIO}
          />
        </TableRowCell>

        <TableRowCell
          className={styles.size}
          title={size}
        >
          {formatBytes(size)}
        </TableRowCell>

        <TableRowCell
          className={styles.language}
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
        </TableRowCell>

        <TableRowCell
          className={styles.quality}
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
        </TableRowCell>

        <TableRowCell
          className={styles.releaseGroup}
        >
          {releaseGroup}
        </TableRowCell>

        <TableRowCell
          className={styles.formats}
        >
          <MovieFormats
            formats={customFormats}
          />
        </TableRowCell>

        <TableRowCell className={styles.actions}>
          <IconButton
            title={translate('EditMovieFile')}
            name={icons.EDIT}
            onPress={this.onFileEditPress}
          />

          <IconButton
            title={translate('Details')}
            name={icons.MEDIA_INFO}
            onPress={this.onFileDetailsPress}
          />

          <IconButton
            title={translate('DeleteFile')}
            name={icons.REMOVE}
            onPress={this.onDeletePress}
          />
        </TableRowCell>

        <FileDetailsModal
          isOpen={isFileDetailsModalOpen}
          onModalClose={this.onFileDetailsModalClose}
          mediaInfo={mediaInfo}
        />

        <FileEditModal
          movieFileId={id}
          isOpen={isFileEditModalOpen}
          onModalClose={this.onFileEditModalClose}
        />

        <ConfirmModal
          isOpen={isConfirmDeleteModalOpen}
          ids={[id]}
          kind={kinds.DANGER}
          title={translate('DeleteSelectedMovieFiles')}
          message={translate('DeleteSelectedMovieFilesMessage')}
          confirmLabel={translate('Delete')}
          onConfirm={this.onConfirmDelete}
          onCancel={this.onConfirmDeleteModalClose}
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
  releaseGroup: PropTypes.string,
  customFormats: PropTypes.arrayOf(PropTypes.object).isRequired,
  qualityCutoffNotMet: PropTypes.bool.isRequired,
  languages: PropTypes.arrayOf(PropTypes.object).isRequired,
  mediaInfo: PropTypes.object,
  onDeletePress: PropTypes.func.isRequired
};

export default MovieFileEditorRow;
