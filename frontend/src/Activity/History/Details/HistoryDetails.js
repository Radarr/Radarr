import PropTypes from 'prop-types';
import React from 'react';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';
import DescriptionListItemDescription from 'Components/DescriptionList/DescriptionListItemDescription';
import DescriptionListItemTitle from 'Components/DescriptionList/DescriptionListItemTitle';
import Link from 'Components/Link/Link';
import formatDateTime from 'Utilities/Date/formatDateTime';
import formatAge from 'Utilities/Number/formatAge';
import translate from 'Utilities/String/translate';
import styles from './HistoryDetails.css';

function HistoryDetails(props) {
  const {
    eventType,
    sourceTitle,
    data,
    downloadId,
    shortDateFormat,
    timeFormat
  } = props;

  if (eventType === 'grabbed') {
    const {
      indexer,
      releaseGroup,
      nzbInfoUrl,
      downloadClient,
      downloadClientName,
      movieMatchType,
      age,
      ageHours,
      ageMinutes,
      publishedDate
    } = data;

    const downloadClientNameInfo = downloadClientName ?? downloadClient;

    return (
      <DescriptionList>
        <DescriptionListItem
          descriptionClassName={styles.description}
          title={translate('Name')}
          data={sourceTitle}
        />

        {
          indexer ?
            <DescriptionListItem
              title={translate('Indexer')}
              data={indexer}
            /> :
            null
        }

        {
          releaseGroup ?
            <DescriptionListItem
              descriptionClassName={styles.description}
              title={translate('ReleaseGroup')}
              data={releaseGroup}
            /> :
            null
        }

        {
          nzbInfoUrl ?
            <span>
              <DescriptionListItemTitle>
                Info URL
              </DescriptionListItemTitle>

              <DescriptionListItemDescription>
                <Link to={nzbInfoUrl}>{nzbInfoUrl}</Link>
              </DescriptionListItemDescription>
            </span> :
            null
        }

        {
          movieMatchType ?
            <DescriptionListItem
              descriptionClassName={styles.description}
              title={translate('MovieMatchType')}
              data={movieMatchType}
            /> :
            null
        }

        {
          downloadClientNameInfo ?
            <DescriptionListItem
              title={translate('DownloadClient')}
              data={downloadClientNameInfo}
            /> :
            null
        }

        {
          downloadId ?
            <DescriptionListItem
              title={translate('GrabID')}
              data={downloadId}
            /> :
            null
        }

        {
          indexer ?
            <DescriptionListItem
              title={translate('AgeWhenGrabbed')}
              data={formatAge(age, ageHours, ageMinutes)}
            /> :
            null
        }

        {
          publishedDate ?
            <DescriptionListItem
              title={translate('PublishedDate')}
              data={formatDateTime(publishedDate, shortDateFormat, timeFormat, { includeSeconds: true })}
            /> :
            null
        }
      </DescriptionList>
    );
  }

  if (eventType === 'downloadFailed') {
    const {
      message
    } = data;

    return (
      <DescriptionList>
        <DescriptionListItem
          descriptionClassName={styles.description}
          title={translate('Name')}
          data={sourceTitle}
        />

        {
          downloadId ?
            <DescriptionListItem
              title={translate('GrabID')}
              data={downloadId}
            /> :
            null
        }

        {
          message ?
            <DescriptionListItem
              title={translate('Message')}
              data={message}
            /> :
            null
        }
      </DescriptionList>
    );
  }

  if (eventType === 'downloadFolderImported') {
    const {
      droppedPath,
      importedPath
    } = data;

    return (
      <DescriptionList>
        <DescriptionListItem
          descriptionClassName={styles.description}
          title={translate('Name')}
          data={sourceTitle}
        />

        {
          droppedPath ?
            <DescriptionListItem
              descriptionClassName={styles.description}
              title={translate('Source')}
              data={droppedPath}
            /> :
            null
        }

        {
          importedPath ?
            <DescriptionListItem
              descriptionClassName={styles.description}
              title={translate('ImportedTo')}
              data={importedPath}
            /> :
            null
        }
      </DescriptionList>
    );
  }

  if (eventType === 'movieFileDeleted') {
    const {
      reason
    } = data;

    let reasonMessage = '';

    switch (reason) {
      case 'Manual':
        reasonMessage = translate('FileWasDeletedByViaUI');
        break;
      case 'MissingFromDisk':
        reasonMessage = translate('MissingFromDisk');
        break;
      case 'Upgrade':
        reasonMessage = translate('FileWasDeletedByUpgrade');
        break;
      default:
        reasonMessage = '';
    }

    return (
      <DescriptionList>
        <DescriptionListItem
          title={translate('Name')}
          data={sourceTitle}
        />

        <DescriptionListItem
          title={translate('Reason')}
          data={reasonMessage}
        />
      </DescriptionList>
    );
  }

  if (eventType === 'movieFileRenamed') {
    const {
      sourcePath,
      sourceRelativePath,
      path,
      relativePath
    } = data;

    return (
      <DescriptionList>
        <DescriptionListItem
          title={translate('SourcePath')}
          data={sourcePath}
        />

        <DescriptionListItem
          title={translate('SourceRelativePath')}
          data={sourceRelativePath}
        />

        <DescriptionListItem
          title={translate('DestinationPath')}
          data={path}
        />

        <DescriptionListItem
          title={translate('DestinationRelativePath')}
          data={relativePath}
        />
      </DescriptionList>
    );
  }

  if (eventType === 'downloadIgnored') {
    const {
      message
    } = data;

    return (
      <DescriptionList>
        <DescriptionListItem
          descriptionClassName={styles.description}
          title={translate('Name')}
          data={sourceTitle}
        />

        {
          downloadId ?
            <DescriptionListItem
              title={translate('GrabID')}
              data={downloadId}
            /> :
            null
        }

        {
          message ?
            <DescriptionListItem
              title={translate('Message')}
              data={message}
            /> :
            null
        }
      </DescriptionList>
    );
  }

  return (
    <DescriptionList>
      <DescriptionListItem
        descriptionClassName={styles.description}
        title={translate('Name')}
        data={sourceTitle}
      />
    </DescriptionList>
  );
}

HistoryDetails.propTypes = {
  eventType: PropTypes.string.isRequired,
  sourceTitle: PropTypes.string.isRequired,
  data: PropTypes.object.isRequired,
  downloadId: PropTypes.string,
  shortDateFormat: PropTypes.string.isRequired,
  timeFormat: PropTypes.string.isRequired
};

export default HistoryDetails;
