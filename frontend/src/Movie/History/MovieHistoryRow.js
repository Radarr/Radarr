import PropTypes from 'prop-types';
import React, { Component } from 'react';
import HistoryDetailsModal from 'Activity/History/Details/HistoryDetailsModal';
import HistoryEventTypeCell from 'Activity/History/HistoryEventTypeCell';
import IconButton from 'Components/Link/IconButton';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import RelativeDateCell from 'Components/Table/Cells/RelativeDateCell';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableRow from 'Components/Table/TableRow';
import { icons, kinds } from 'Helpers/Props';
import MovieFormats from 'Movie/MovieFormats';
import MovieLanguages from 'Movie/MovieLanguages';
import MovieQuality from 'Movie/MovieQuality';
import formatCustomFormatScore from 'Utilities/Number/formatCustomFormatScore';
import translate from 'Utilities/String/translate';
import styles from './MovieHistoryRow.css';

class MovieHistoryRow extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isMarkAsFailedModalOpen: false,
      isDetailsModalOpen: false
    };
  }

  //
  // Listeners

  onMarkAsFailedPress = () => {
    this.setState({ isMarkAsFailedModalOpen: true });
  };

  onConfirmMarkAsFailed = () => {
    this.props.onMarkAsFailedPress(this.props.id);
    this.setState({ isMarkAsFailedModalOpen: false });
  };

  onMarkAsFailedModalClose = () => {
    this.setState({ isMarkAsFailedModalOpen: false });
  };

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
      eventType,
      sourceTitle,
      quality,
      customFormats,
      customFormatScore,
      languages,
      qualityCutoffNotMet,
      date,
      data,
      downloadId,
      isMarkingAsFailed,
      shortDateFormat,
      timeFormat
    } = this.props;

    const {
      isMarkAsFailedModalOpen
    } = this.state;

    return (
      <TableRow>
        <HistoryEventTypeCell
          eventType={eventType}
          data={data}
        />

        <TableRowCell className={styles.sourceTitle}>
          {sourceTitle}
        </TableRowCell>

        <TableRowCell>
          <MovieLanguages
            languages={languages}
          />
        </TableRowCell>

        <TableRowCell>
          <MovieQuality
            quality={quality}
            isCutoffNotMet={qualityCutoffNotMet}
          />
        </TableRowCell>

        <TableRowCell>
          <MovieFormats formats={customFormats} />
        </TableRowCell>

        <TableRowCell className={styles.customFormatScore}>
          {formatCustomFormatScore(customFormatScore, customFormats.length)}
        </TableRowCell>

        <RelativeDateCell
          date={date}
          includeSeconds={true}
          includeTime={true}
        />

        <TableRowCell className={styles.actions}>
          <IconButton
            name={icons.INFO}
            onPress={this.onDetailsPress}
          />

          {
            eventType === 'grabbed' &&
              <IconButton
                title={translate('MarkAsFailed')}
                name={icons.REMOVE}
                size={14}
                onPress={this.onMarkAsFailedPress}
              />
          }
        </TableRowCell>

        <ConfirmModal
          isOpen={isMarkAsFailedModalOpen}
          kind={kinds.DANGER}
          title={translate('MarkAsFailed')}
          message={translate('MarkAsFailedMessageText', [sourceTitle])}
          confirmLabel={translate('MarkAsFailed')}
          onConfirm={this.onConfirmMarkAsFailed}
          onCancel={this.onMarkAsFailedModalClose}
        />

        <HistoryDetailsModal
          isOpen={this.state.isDetailsModalOpen}
          eventType={eventType}
          sourceTitle={sourceTitle}
          data={data}
          downloadId={downloadId}
          isMarkingAsFailed={isMarkingAsFailed}
          shortDateFormat={shortDateFormat}
          timeFormat={timeFormat}
          onMarkAsFailedPress={this.onMarkAsFailedPress}
          onModalClose={this.onDetailsModalClose}
        />
      </TableRow>
    );
  }
}

MovieHistoryRow.propTypes = {
  id: PropTypes.number.isRequired,
  eventType: PropTypes.string.isRequired,
  sourceTitle: PropTypes.string.isRequired,
  languages: PropTypes.arrayOf(PropTypes.object).isRequired,
  quality: PropTypes.object.isRequired,
  customFormats: PropTypes.arrayOf(PropTypes.object),
  customFormatScore: PropTypes.number.isRequired,
  qualityCutoffNotMet: PropTypes.bool.isRequired,
  date: PropTypes.string.isRequired,
  data: PropTypes.object.isRequired,
  downloadId: PropTypes.string,
  isMarkingAsFailed: PropTypes.bool,
  movie: PropTypes.object.isRequired,
  shortDateFormat: PropTypes.string.isRequired,
  timeFormat: PropTypes.string.isRequired,
  onMarkAsFailedPress: PropTypes.func.isRequired
};

export default MovieHistoryRow;
