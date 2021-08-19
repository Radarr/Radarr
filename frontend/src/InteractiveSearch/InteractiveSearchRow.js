import PropTypes from 'prop-types';
import React, { Component } from 'react';
import ProtocolLabel from 'Activity/Queue/ProtocolLabel';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableRow from 'Components/Table/TableRow';
import Popover from 'Components/Tooltip/Popover';
import { icons, kinds, tooltipPositions } from 'Helpers/Props';
import MovieFormats from 'Movie/MovieFormats';
import MovieLanguage from 'Movie/MovieLanguage';
import MovieQuality from 'Movie/MovieQuality';
import formatDateTime from 'Utilities/Date/formatDateTime';
import formatAge from 'Utilities/Number/formatAge';
import formatBytes from 'Utilities/Number/formatBytes';
import translate from 'Utilities/String/translate';
import Peers from './Peers';
import styles from './InteractiveSearchRow.css';

function getDownloadIcon(isGrabbing, isGrabbed, grabError) {
  if (isGrabbing) {
    return icons.SPINNER;
  } else if (isGrabbed) {
    return icons.DOWNLOADING;
  } else if (grabError) {
    return icons.DOWNLOADING;
  }

  return icons.DOWNLOAD;
}

function getDownloadTooltip(isGrabbing, isGrabbed, grabError) {
  if (isGrabbing) {
    return '';
  } else if (isGrabbed) {
    return translate('AddedToDownloadQueue');
  } else if (grabError) {
    return grabError;
  }

  return translate('AddToDownloadQueue');
}

class InteractiveSearchRow extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isConfirmGrabModalOpen: false
    };
  }

  //
  // Listeners

  onGrabPress = () => {
    const {
      guid,
      indexerId,
      onGrabPress
    } = this.props;

    onGrabPress({
      guid,
      indexerId
    });
  }

  onConfirmGrabPress = () => {
    this.setState({ isConfirmGrabModalOpen: true });
  }

  onGrabConfirm = () => {
    this.setState({ isConfirmGrabModalOpen: false });

    const {
      guid,
      indexerId,
      searchPayload,
      onGrabPress
    } = this.props;

    onGrabPress({
      guid,
      indexerId,
      ...searchPayload
    });
  }

  onGrabCancel = () => {
    this.setState({ isConfirmGrabModalOpen: false });
  }

  //
  // Render

  render() {
    const {
      protocol,
      age,
      ageHours,
      ageMinutes,
      publishDate,
      title,
      infoUrl,
      indexer,
      size,
      seeders,
      leechers,
      quality,
      customFormats,
      customFormatScore,
      languages,
      indexerFlags,
      rejections,
      downloadAllowed,
      isGrabbing,
      isGrabbed,
      longDateFormat,
      timeFormat,
      grabError,
      historyGrabbedData,
      historyFailedData,
      blocklistData
    } = this.props;

    return (
      <TableRow>
        <TableRowCell className={styles.protocol}>
          <ProtocolLabel
            protocol={protocol}
          />
        </TableRowCell>

        <TableRowCell
          className={styles.age}
          title={formatDateTime(publishDate, longDateFormat, timeFormat, { includeSeconds: true })}
        >
          {formatAge(age, ageHours, ageMinutes)}
        </TableRowCell>

        <TableRowCell className={styles.download}>
          <SpinnerIconButton
            name={getDownloadIcon(isGrabbing, isGrabbed, grabError)}
            kind={grabError ? kinds.DANGER : kinds.DEFAULT}
            title={getDownloadTooltip(isGrabbing, isGrabbed, grabError)}
            isDisabled={isGrabbed}
            isSpinning={isGrabbing}
            onPress={downloadAllowed ? this.onGrabPress : this.onConfirmGrabPress}
          />
        </TableRowCell>

        <TableRowCell className={styles.rejected}>
          {
            !!rejections.length &&
              <Popover
                anchor={
                  <Icon
                    name={icons.DANGER}
                    kind={kinds.DANGER}
                  />
                }
                title={translate('ReleaseRejected')}
                body={
                  <ul>
                    {
                      rejections.map((rejection, index) => {
                        return (
                          <li key={index}>
                            {rejection}
                          </li>
                        );
                      })
                    }
                  </ul>
                }
                position={tooltipPositions.BOTTOM}
              />
          }
        </TableRowCell>

        <TableRowCell className={styles.title}>
          <Link
            to={infoUrl}
            title={title}
          >
            <div>
              {title}
            </div>
          </Link>
        </TableRowCell>

        <TableRowCell className={styles.indexer}>
          {indexer}
        </TableRowCell>

        <TableRowCell className={styles.history}>
          {
            historyGrabbedData?.date && !historyFailedData?.date &&
              <Icon
                name={icons.DOWNLOADING}
                kind={kinds.DEFAULT}
                title={`${translate('Grabbed')}: ${formatDateTime(historyGrabbedData.date, longDateFormat, timeFormat, { includeSeconds: true })}`}
              />
          }

          {
            historyFailedData?.date &&
              <Icon
                className={styles.failed}
                name={icons.DOWNLOADING}
                kind={kinds.DANGER}
                title={`${translate('Failed')}: ${formatDateTime(historyFailedData.date, longDateFormat, timeFormat, { includeSeconds: true })}`}
              />
          }

          {
            blocklistData?.date &&
              <Icon
                className={historyGrabbedData || historyFailedData ? styles.blocklist : ''}
                name={icons.BLOCKLIST}
                kind={kinds.DANGER}
                title={`${translate('Blocklisted')}: ${formatDateTime(blocklistData.date, longDateFormat, timeFormat, { includeSeconds: true })}`}
              />
          }
        </TableRowCell>

        <TableRowCell className={styles.size}>
          {formatBytes(size)}
        </TableRowCell>

        <TableRowCell className={styles.peers}>
          {
            protocol === 'torrent' &&
              <Peers
                seeders={seeders}
                leechers={leechers}
              />
          }
        </TableRowCell>

        <TableRowCell className={styles.language}>
          <MovieLanguage
            languages={languages}
          />
        </TableRowCell>

        <TableRowCell className={styles.quality}>
          <MovieQuality
            quality={quality}
          />
        </TableRowCell>

        <TableRowCell className={styles.customFormat}>
          <MovieFormats
            formats={customFormats}
          />
        </TableRowCell>

        <TableRowCell className={styles.customFormatScore}>
          {customFormatScore > 0 && `+${customFormatScore}`}
          {customFormatScore < 0 && customFormatScore}
        </TableRowCell>

        <TableRowCell className={styles.indexerFlags}>
          {
            !!indexerFlags.length &&
              <Popover
                anchor={
                  <Icon
                    name={icons.FLAG}
                    kind={kinds.PRIMARY}
                  />
                }
                title={translate('IndexerFlags')}
                body={
                  <ul>
                    {
                      indexerFlags.map((flag, index) => {
                        return (
                          <li key={index}>
                            {flag}
                          </li>
                        );
                      })
                    }
                  </ul>
                }
                position={tooltipPositions.BOTTOM}
              />
          }
        </TableRowCell>

        <ConfirmModal
          isOpen={this.state.isConfirmGrabModalOpen}
          kind={kinds.WARNING}
          title={translate('GrabRelease')}
          message={translate('GrabReleaseMessageText', [title])}
          confirmLabel={translate('Grab')}
          onConfirm={this.onGrabConfirm}
          onCancel={this.onGrabCancel}
        />
      </TableRow>
    );
  }
}

InteractiveSearchRow.propTypes = {
  guid: PropTypes.string.isRequired,
  protocol: PropTypes.string.isRequired,
  age: PropTypes.number.isRequired,
  ageHours: PropTypes.number.isRequired,
  ageMinutes: PropTypes.number.isRequired,
  publishDate: PropTypes.string.isRequired,
  title: PropTypes.string.isRequired,
  infoUrl: PropTypes.string.isRequired,
  indexerId: PropTypes.number.isRequired,
  indexer: PropTypes.string.isRequired,
  size: PropTypes.number.isRequired,
  seeders: PropTypes.number,
  leechers: PropTypes.number,
  quality: PropTypes.object.isRequired,
  customFormats: PropTypes.arrayOf(PropTypes.object).isRequired,
  customFormatScore: PropTypes.number.isRequired,
  languages: PropTypes.arrayOf(PropTypes.object).isRequired,
  rejections: PropTypes.arrayOf(PropTypes.string).isRequired,
  indexerFlags: PropTypes.arrayOf(PropTypes.string).isRequired,
  downloadAllowed: PropTypes.bool.isRequired,
  isGrabbing: PropTypes.bool.isRequired,
  isGrabbed: PropTypes.bool.isRequired,
  grabError: PropTypes.string,
  longDateFormat: PropTypes.string.isRequired,
  timeFormat: PropTypes.string.isRequired,
  searchPayload: PropTypes.object.isRequired,
  onGrabPress: PropTypes.func.isRequired,
  historyFailedData: PropTypes.object,
  historyGrabbedData: PropTypes.object,
  blocklistData: PropTypes.object
};

InteractiveSearchRow.defaultProps = {
  rejections: [],
  isGrabbing: false,
  isGrabbed: false
};

export default InteractiveSearchRow;
