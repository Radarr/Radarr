import PropTypes from 'prop-types';
import React, { Component } from 'react';
import IconButton from 'Components/Link/IconButton';
import RelativeDateCellConnector from 'Components/Table/Cells/RelativeDateCellConnector';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableRow from 'Components/Table/TableRow';
import Tooltip from 'Components/Tooltip/Tooltip';
import { icons, tooltipPositions } from 'Helpers/Props';
import MovieFormats from 'Movie/MovieFormats';
import MovieLanguages from 'Movie/MovieLanguages';
import MovieQuality from 'Movie/MovieQuality';
import MovieTitleLink from 'Movie/MovieTitleLink';
import formatCustomFormatScore from 'Utilities/Number/formatCustomFormatScore';
import HistoryDetailsModal from './Details/HistoryDetailsModal';
import HistoryEventTypeCell from './HistoryEventTypeCell';
import styles from './HistoryRow.css';

class HistoryRow extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isDetailsModalOpen: false
    };
  }

  componentDidUpdate(prevProps) {
    if (
      prevProps.isMarkingAsFailed &&
      !this.props.isMarkingAsFailed &&
      !this.props.markAsFailedError
    ) {
      this.setState({ isDetailsModalOpen: false });
    }
  }

  //
  // Listeners

  onDetailsPress = () => {
    this.setState({ isDetailsModalOpen: true });
  };

  onDetailsModalClose = () => {
    this.setState({ isDetailsModalOpen: false });
  };

  //
  // Render

  render() {
    const {
      movie,
      quality,
      customFormats,
      customFormatScore,
      languages,
      qualityCutoffNotMet,
      eventType,
      sourceTitle,
      date,
      data,
      downloadId,
      isMarkingAsFailed,
      columns,
      shortDateFormat,
      timeFormat,
      onMarkAsFailedPress
    } = this.props;

    if (!movie) {
      return null;
    }

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

            if (name === 'eventType') {
              return (
                <HistoryEventTypeCell
                  key={name}
                  eventType={eventType}
                  data={data}
                />
              );
            }

            if (name === 'movieMetadata.sortTitle') {
              return (
                <TableRowCell key={name}>
                  <MovieTitleLink
                    titleSlug={movie.titleSlug}
                    title={movie.title}
                  />
                </TableRowCell>
              );
            }

            if (name === 'languages') {
              return (
                <TableRowCell key={name}>
                  <MovieLanguages
                    languages={languages}
                  />
                </TableRowCell>
              );
            }

            if (name === 'quality') {
              return (
                <TableRowCell key={name}>
                  <MovieQuality
                    quality={quality}
                    isCutoffMet={qualityCutoffNotMet}
                  />
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

            if (name === 'date') {
              return (
                <RelativeDateCellConnector
                  key={name}
                  date={date}
                />
              );
            }

            if (name === 'downloadClient') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.downloadClient}
                >
                  {data.downloadClient}
                </TableRowCell>
              );
            }

            if (name === 'indexer') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.indexer}
                >
                  {data.indexer}
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
                    position={tooltipPositions.BOTTOM}
                  />
                </TableRowCell>
              );
            }

            if (name === 'releaseGroup') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.releaseGroup}
                >
                  {data.releaseGroup}
                </TableRowCell>
              );
            }

            if (name === 'sourceTitle') {
              return (
                <TableRowCell
                  key={name}
                >
                  {sourceTitle}
                </TableRowCell>
              );
            }

            if (name === 'details') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.details}
                >
                  <div className={styles.actionContents}>
                    <IconButton
                      name={icons.INFO}
                      onPress={this.onDetailsPress}
                    />
                  </div>
                </TableRowCell>
              );
            }

            return null;
          })
        }

        <HistoryDetailsModal
          isOpen={this.state.isDetailsModalOpen}
          eventType={eventType}
          sourceTitle={sourceTitle}
          data={data}
          downloadId={downloadId}
          isMarkingAsFailed={isMarkingAsFailed}
          shortDateFormat={shortDateFormat}
          timeFormat={timeFormat}
          onMarkAsFailedPress={onMarkAsFailedPress}
          onModalClose={this.onDetailsModalClose}
        />
      </TableRow>
    );
  }

}

HistoryRow.propTypes = {
  movieId: PropTypes.number,
  movie: PropTypes.object.isRequired,
  languages: PropTypes.arrayOf(PropTypes.object).isRequired,
  quality: PropTypes.object.isRequired,
  customFormats: PropTypes.arrayOf(PropTypes.object),
  customFormatScore: PropTypes.number.isRequired,
  qualityCutoffNotMet: PropTypes.bool.isRequired,
  eventType: PropTypes.string.isRequired,
  sourceTitle: PropTypes.string.isRequired,
  date: PropTypes.string.isRequired,
  data: PropTypes.object.isRequired,
  downloadId: PropTypes.string,
  isMarkingAsFailed: PropTypes.bool,
  markAsFailedError: PropTypes.object,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  shortDateFormat: PropTypes.string.isRequired,
  timeFormat: PropTypes.string.isRequired,
  onMarkAsFailedPress: PropTypes.func.isRequired
};

HistoryRow.defaultProps = {
  customFormats: []
};

export default HistoryRow;
