import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Icon from 'Components/Icon';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableRowCellButton from 'Components/Table/Cells/TableRowCellButton';
import TableSelectCell from 'Components/Table/Cells/TableSelectCell';
import TableRow from 'Components/Table/TableRow';
import Popover from 'Components/Tooltip/Popover';
import { icons, kinds, tooltipPositions } from 'Helpers/Props';
import SelectLanguageModal from 'InteractiveImport/Language/SelectLanguageModal';
import SelectMovieModal from 'InteractiveImport/Movie/SelectMovieModal';
import SelectQualityModal from 'InteractiveImport/Quality/SelectQualityModal';
import MovieLanguage from 'Movie/MovieLanguage';
import MovieQuality from 'Movie/MovieQuality';
import formatBytes from 'Utilities/Number/formatBytes';
import InteractiveImportRowCellPlaceholder from './InteractiveImportRowCellPlaceholder';
import styles from './InteractiveImportRow.css';

class InteractiveImportRow extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isSelectMovieModalOpen: false,
      isSelectQualityModalOpen: false,
      isSelectLanguageModalOpen: false
    };
  }

  componentDidMount() {
    const {
      id,
      movie,
      quality,
      languages
    } = this.props;

    if (
      movie &&
      quality &&
      languages
    ) {
      this.props.onSelectedChange({ id, value: true });
    }
  }

  componentDidUpdate(prevProps) {
    const {
      id,
      movie,
      quality,
      languages,
      isSelected,
      onValidRowChange
    } = this.props;

    if (
      prevProps.movie === movie &&
      prevProps.quality === quality &&
      prevProps.languages === languages &&
      prevProps.isSelected === isSelected
    ) {
      return;
    }

    const isValid = !!(
      movie &&
      quality &&
      languages
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

  onSelectMoviePress = () => {
    this.setState({ isSelectMovieModalOpen: true });
  }

  onSelectQualityPress = () => {
    this.setState({ isSelectQualityModalOpen: true });
  }

  onSelectLanguagePress = () => {
    this.setState({ isSelectLanguageModalOpen: true });
  }

  onSelectMovieModalClose = (changed) => {
    this.setState({ isSelectMovieModalOpen: false });
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
      allowMovieChange,
      relativePath,
      movie,
      quality,
      languages,
      size,
      rejections,
      isReprocessing,
      isSelected,
      onSelectedChange
    } = this.props;

    const {
      isSelectMovieModalOpen,
      isSelectQualityModalOpen,
      isSelectLanguageModalOpen
    } = this.state;

    const movieTitle = movie ? movie.title : '';

    const showMoviePlaceholder = isSelected && !movie;
    const showQualityPlaceholder = isSelected && !quality;
    const showLanguagePlaceholder = isSelected && !languages && !isReprocessing;

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
          isDisabled={!allowMovieChange}
          title={allowMovieChange ? 'Click to change movie' : undefined}
          onPress={this.onSelectMoviePress}
        >
          {
            showMoviePlaceholder ? <InteractiveImportRowCellPlaceholder /> : movieTitle
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
              <MovieQuality
                className={styles.label}
                quality={quality}
              />
          }
        </TableRowCellButton>

        <TableRowCellButton
          className={styles.language}
          title="Click to change language"
          onPress={this.onSelectLanguagePress}
        >
          {
            showLanguagePlaceholder &&
              <InteractiveImportRowCellPlaceholder />
          }

          {
            !showLanguagePlaceholder && !!languages && !isReprocessing ?
              <MovieLanguage
                className={styles.label}
                languages={languages}
              /> :
              null
          }

          {
            isReprocessing ?
              <LoadingIndicator className={styles.reprocessing}
                size={20}

              /> : null
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

        <SelectMovieModal
          isOpen={isSelectMovieModalOpen}
          ids={[id]}
          relativePath={relativePath}
          onModalClose={this.onSelectMovieModalClose}
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
          languageIds={languages ? languages.map((l) => l.id) : []}
          onModalClose={this.onSelectLanguageModalClose}
        />
      </TableRow>
    );
  }

}

InteractiveImportRow.propTypes = {
  id: PropTypes.number.isRequired,
  allowMovieChange: PropTypes.bool.isRequired,
  relativePath: PropTypes.string.isRequired,
  movie: PropTypes.object,
  quality: PropTypes.object,
  languages: PropTypes.arrayOf(PropTypes.object),
  size: PropTypes.number.isRequired,
  rejections: PropTypes.arrayOf(PropTypes.object).isRequired,
  isReprocessing: PropTypes.bool,
  isSelected: PropTypes.bool,
  onSelectedChange: PropTypes.func.isRequired,
  onValidRowChange: PropTypes.func.isRequired
};

export default InteractiveImportRow;
