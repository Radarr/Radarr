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
import MovieQuality from 'Movie/MovieQuality';
// import EpisodeLanguage from 'Episode/EpisodeLanguage';
import SelectSeriesModal from 'InteractiveImport/Series/SelectSeriesModal';
import SelectQualityModal from 'InteractiveImport/Quality/SelectQualityModal';
import InteractiveImportRowCellPlaceholder from './InteractiveImportRowCellPlaceholder';
import styles from './InteractiveImportRow.css';

class InteractiveImportRow extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isSelectSeriesModalOpen: false,
      isSelectQualityModalOpen: false
    };
  }

  componentDidMount() {
    const {
      id,
      series,
      quality
    } = this.props;

    if (
      series &&
      quality
    ) {
      this.props.onSelectedChange({ id, value: true });
    }
  }

  componentDidUpdate(prevProps) {
    const {
      id,
      series,
      quality,
      isSelected,
      onValidRowChange
    } = this.props;

    if (
      prevProps.series === series &&
      prevProps.quality === quality &&
      prevProps.isSelected === isSelected
    ) {
      return;
    }

    const isValid = !!(
      series &&
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

  onSelectSeriesPress = () => {
    this.setState({ isSelectSeriesModalOpen: true });
  }

  onSelectQualityPress = () => {
    this.setState({ isSelectQualityModalOpen: true });
  }

  onSelectSeriesModalClose = (changed) => {
    this.setState({ isSelectSeriesModalOpen: false });
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
      allowSeriesChange,
      relativePath,
      series,
      quality,
      size,
      rejections,
      isSelected,
      onSelectedChange
    } = this.props;

    const {
      isSelectSeriesModalOpen,
      isSelectQualityModalOpen
    } = this.state;

    const seriesTitle = series ? series.title : '';

    const showSeriesPlaceholder = isSelected && !series;
    const showQualityPlaceholder = isSelected && !quality;

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
          isDisabled={!allowSeriesChange}
          onPress={this.onSelectSeriesPress}
        >
          {
            showSeriesPlaceholder ? <InteractiveImportRowCellPlaceholder /> : seriesTitle
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
              <MovieQuality
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

        <SelectSeriesModal
          isOpen={isSelectSeriesModalOpen}
          ids={[id]}
          onModalClose={this.onSelectSeriesModalClose}
        />

        <SelectQualityModal
          isOpen={isSelectQualityModalOpen}
          id={id}
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
  allowSeriesChange: PropTypes.bool.isRequired,
  relativePath: PropTypes.string.isRequired,
  series: PropTypes.object,
  seasonNumber: PropTypes.number,
  episodes: PropTypes.arrayOf(PropTypes.object).isRequired,
  quality: PropTypes.object,
  size: PropTypes.number.isRequired,
  rejections: PropTypes.arrayOf(PropTypes.object).isRequired,
  isSelected: PropTypes.bool,
  onSelectedChange: PropTypes.func.isRequired,
  onValidRowChange: PropTypes.func.isRequired
};

InteractiveImportRow.defaultProps = {
  episodes: []
};

export default InteractiveImportRow;
