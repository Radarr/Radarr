import PropTypes from 'prop-types';
import React, { Component } from 'react';
import IconButton from 'Components/Link/IconButton';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableRow from 'Components/Table/TableRow';
import Tooltip from 'Components/Tooltip/Tooltip';
import { icons, kinds, tooltipPositions } from 'Helpers/Props';
import MovieFormats from 'Movie/MovieFormats';
import MovieLanguage from 'Movie/MovieLanguage';
import MovieQuality from 'Movie/MovieQuality';
import FileEditModal from 'MovieFile/Edit/FileEditModal';
import MediaInfoConnector from 'MovieFile/MediaInfoConnector';
import * as mediaInfoTypes from 'MovieFile/mediaInfoTypes';
import formatBytes from 'Utilities/Number/formatBytes';
import formatCustomFormatScore from 'Utilities/Number/formatCustomFormatScore';
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
      customFormatScore,
      languages,
      columns
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
        {
          columns.map((column) => {
            const {
              name,
              isVisible
            } = column;

            if (!isVisible) {
              return null;
            }

            if (name === 'relativePath') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.relativePath}
                  title={relativePath}
                >
                  {relativePath}
                </TableRowCell>
              );
            }

            if (name === 'customFormats') {
              return (
                <TableRowCell key={name}>
                  <MovieFormats
                    formats={customFormats}
                  />
                </TableRowCell>
              );
            }

            if (name === 'customFormatScore') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.customFormatScore}
                >
                  <Tooltip
                    anchor={formatCustomFormatScore(
                      customFormatScore,
                      customFormats.length
                    )}
                    tooltip={<MovieFormats formats={customFormats} />}
                    position={tooltipPositions.TOP}
                  />
                </TableRowCell>
              );
            }

            if (name === 'languages') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.languages}
                >
                  {
                    showLanguagePlaceholder ?
                      <MovieFileRowCellPlaceholder /> :
                      null
                  }

                  {
                    !showLanguagePlaceholder && !!languages &&
                      <MovieLanguage
                        className={styles.label}
                        languages={languages}
                      />
                  }
                </TableRowCell>
              );
            }

            if (name === 'quality') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.quality}
                >
                  {
                    showQualityPlaceholder ?
                      <MovieFileRowCellPlaceholder /> :
                      null
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
              );
            }

            if (name === 'audioInfo') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.audio}
                >
                  <MediaInfoConnector
                    type={mediaInfoTypes.AUDIO}
                    movieFileId={id}
                  />
                </TableRowCell>
              );
            }

            if (name === 'audioLanguages') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.audioLanguages}
                >
                  <MediaInfoConnector
                    type={mediaInfoTypes.AUDIO_LANGUAGES}
                    movieFileId={id}
                  />
                </TableRowCell>
              );
            }

            if (name === 'subtitleLanguages') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.subtitles}
                >
                  <MediaInfoConnector
                    type={mediaInfoTypes.SUBTITLES}
                    movieFileId={id}
                  />
                </TableRowCell>
              );
            }

            if (name === 'videoCodec') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.video}
                >
                  <MediaInfoConnector
                    type={mediaInfoTypes.VIDEO}
                    movieFileId={id}
                  />
                </TableRowCell>
              );
            }

            if (name === 'videoDynamicRangeType') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.videoDynamicRangeType}
                >
                  <MediaInfoConnector
                    type={mediaInfoTypes.VIDEO_DYNAMIC_RANGE_TYPE}
                    movieFileId={id}
                  />
                </TableRowCell>
              );
            }

            if (name === 'size') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.size}
                  title={size}
                >
                  {formatBytes(size)}
                </TableRowCell>
              );
            }

            if (name === 'releaseGroup') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.releaseGroup}
                >
                  {releaseGroup}
                </TableRowCell>
              );
            }

            if (name === 'actions') {
              return (
                <TableRowCell key={name} className={styles.actions}>
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
              );
            }

            return null;
          })
        }

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
          message={translate('DeleteSelectedMovieFilesHelpText')}
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
  customFormatScore: PropTypes.number.isRequired,
  qualityCutoffNotMet: PropTypes.bool.isRequired,
  languages: PropTypes.arrayOf(PropTypes.object).isRequired,
  mediaInfo: PropTypes.object,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  onDeletePress: PropTypes.func.isRequired
};

MovieFileEditorRow.defaultProps = {
  customFormats: []
};

export default MovieFileEditorRow;
