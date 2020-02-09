import PropTypes from 'prop-types';
import React from 'react';
import formatDateTime from 'Utilities/Date/formatDateTime';
import formatAge from 'Utilities/Number/formatAge';
import Link from 'Components/Link/Link';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';
import DescriptionListItemTitle from 'Components/DescriptionList/DescriptionListItemTitle';
import DescriptionListItemDescription from 'Components/DescriptionList/DescriptionListItemDescription';
import { icons } from 'Helpers/Props';
import Icon from 'Components/Icon';
import styles from './HistoryDetails.css';

function getDetailedList(statusMessages) {
  return (
    <div>
      {
        statusMessages.map(({ title, messages }) => {
          return (
            <div key={title}>
              {title}
              <ul>
                {
                  messages.map((message) => {
                    return (
                      <li key={message}>
                        {message}
                      </li>
                    );
                  })
                }
              </ul>
            </div>
          );
        })
      }
    </div>
  );
}

function formatMissing(value) {
  if (value === undefined || value === 0 || value === '0') {
    return (<Icon name={icons.BAN} size={12} />);
  }
  return value;
}

function formatChange(oldValue, newValue) {
  return (
    <div> {formatMissing(oldValue)} <Icon name={icons.ARROW_RIGHT_NO_CIRCLE} size={12} /> {formatMissing(newValue)} </div>
  );
}

function HistoryDetails(props) {
  const {
    eventType,
    sourceTitle,
    data,
    shortDateFormat,
    timeFormat
  } = props;

  if (eventType === 'grabbed') {
    const {
      indexer,
      releaseGroup,
      nzbInfoUrl,
      downloadClient,
      downloadId,
      age,
      ageHours,
      ageMinutes,
      publishedDate
    } = data;

    return (
      <DescriptionList>
        <DescriptionListItem
          descriptionClassName={styles.description}
          title="Name"
          data={sourceTitle}
        />

        {
          !!indexer &&
            <DescriptionListItem
              title="Indexer"
              data={indexer}
            />
        }

        {
          !!releaseGroup &&
            <DescriptionListItem
              descriptionClassName={styles.description}
              title="Release Group"
              data={releaseGroup}
            />
        }

        {
          !!nzbInfoUrl &&
            <span>
              <DescriptionListItemTitle>
                Info URL
              </DescriptionListItemTitle>

              <DescriptionListItemDescription>
                <Link to={nzbInfoUrl}>{nzbInfoUrl}</Link>
              </DescriptionListItemDescription>
            </span>
        }

        {
          !!downloadClient &&
            <DescriptionListItem
              title="Download Client"
              data={downloadClient}
            />
        }

        {
          !!downloadId &&
            <DescriptionListItem
              title="Grab ID"
              data={downloadId}
            />
        }

        {
          !!indexer &&
            <DescriptionListItem
              title="Age (when grabbed)"
              data={formatAge(age, ageHours, ageMinutes)}
            />
        }

        {
          !!publishedDate &&
            <DescriptionListItem
              title="Published Date"
              data={formatDateTime(publishedDate, shortDateFormat, timeFormat, { includeSeconds: true })}
            />
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
          title="Name"
          data={sourceTitle}
        />

        {
          !!message &&
            <DescriptionListItem
              title="Message"
              data={message}
            />
        }
      </DescriptionList>
    );
  }

  if (eventType === 'trackFileImported') {
    const {
      droppedPath,
      importedPath
    } = data;

    return (
      <DescriptionList>
        <DescriptionListItem
          descriptionClassName={styles.description}
          title="Name"
          data={sourceTitle}
        />

        {
          !!droppedPath &&
            <DescriptionListItem
              descriptionClassName={styles.description}
              title="Source"
              data={droppedPath}
            />
        }

        {
          !!importedPath &&
            <DescriptionListItem
              descriptionClassName={styles.description}
              title="Imported To"
              data={importedPath}
            />
        }
      </DescriptionList>
    );
  }

  if (eventType === 'trackFileDeleted') {
    const {
      reason
    } = data;

    let reasonMessage = '';

    switch (reason) {
      case 'Manual':
        reasonMessage = 'File was deleted by via UI';
        break;
      case 'MissingFromDisk':
        reasonMessage = 'Lidarr was unable to find the file on disk so it was removed';
        break;
      case 'Upgrade':
        reasonMessage = 'File was deleted to import an upgrade';
        break;
      default:
        reasonMessage = '';
    }

    return (
      <DescriptionList>
        <DescriptionListItem
          title="Name"
          data={sourceTitle}
        />

        <DescriptionListItem
          title="Reason"
          data={reasonMessage}
        />
      </DescriptionList>
    );
  }

  if (eventType === 'trackFileRenamed') {
    const {
      sourcePath,
      path
    } = data;

    return (
      <DescriptionList>
        <DescriptionListItem
          title="Source Path"
          data={sourcePath}
        />

        <DescriptionListItem
          title="Destination Path"
          data={path}
        />
      </DescriptionList>
    );
  }

  if (eventType === 'trackFileRetagged') {
    const {
      diff,
      tagsScrubbed
    } = data;

    return (
      <DescriptionList>
        <DescriptionListItem
          title="Path"
          data={sourceTitle}
        />
        {
          JSON.parse(diff).map(({ field, oldValue, newValue }) => {
            return (
              <DescriptionListItem
                key={field}
                title={field}
                data={formatChange(oldValue, newValue)}
              />
            );
          })
        }
        <DescriptionListItem
          title="Existing tags scrubbed"
          data={tagsScrubbed === 'True' ? <Icon name={icons.CHECK} /> : <Icon name={icons.REMOVE} />}
        />
      </DescriptionList>
    );
  }

  if (eventType === 'albumImportIncomplete') {
    const {
      statusMessages
    } = data;

    return (
      <DescriptionList>
        <DescriptionListItem
          title="Name"
          data={sourceTitle}
        />

        {
          !!statusMessages &&
            <DescriptionListItem
              title="Import failures"
              data={getDetailedList(JSON.parse(statusMessages))}
            />
        }
      </DescriptionList>
    );
  }

  if (eventType === 'downloadImported') {
    const {
      indexer,
      releaseGroup,
      nzbInfoUrl,
      downloadClient,
      downloadId,
      age,
      ageHours,
      ageMinutes,
      publishedDate
    } = data;

    return (
      <DescriptionList>
        <DescriptionListItem
          title="Name"
          data={sourceTitle}
        />

        {
          !!indexer &&
            <DescriptionListItem
              title="Indexer"
              data={indexer}
            />
        }

        {
          !!releaseGroup &&
            <DescriptionListItem
              title="Release Group"
              data={releaseGroup}
            />
        }

        {
          !!nzbInfoUrl &&
            <span>
              <DescriptionListItemTitle>
                Info URL
              </DescriptionListItemTitle>

              <DescriptionListItemDescription>
                <Link to={nzbInfoUrl}>{nzbInfoUrl}</Link>
              </DescriptionListItemDescription>
            </span>
        }

        {
          !!downloadClient &&
            <DescriptionListItem
              title="Download Client"
              data={downloadClient}
            />
        }

        {
          !!downloadId &&
            <DescriptionListItem
              title="Grab ID"
              data={downloadId}
            />
        }

        {
          !!indexer &&
            <DescriptionListItem
              title="Age (when grabbed)"
              data={formatAge(age, ageHours, ageMinutes)}
            />
        }

        {
          !!publishedDate &&
            <DescriptionListItem
              title="Published Date"
              data={formatDateTime(publishedDate, shortDateFormat, timeFormat, { includeSeconds: true })}
            />
        }
      </DescriptionList>
    );
  }

  return (
    <DescriptionList>
      <DescriptionListItem
        descriptionClassName={styles.description}
        title="Name"
        data={sourceTitle}
      />
    </DescriptionList>
  );
}

HistoryDetails.propTypes = {
  eventType: PropTypes.string.isRequired,
  sourceTitle: PropTypes.string.isRequired,
  data: PropTypes.object.isRequired,
  shortDateFormat: PropTypes.string.isRequired,
  timeFormat: PropTypes.string.isRequired
};

export default HistoryDetails;
