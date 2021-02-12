import PropTypes from 'prop-types';
import React from 'react';
import Icon from 'Components/Icon';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import { icons, sortDirections } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import InteractiveSearchRowConnector from './InteractiveSearchRowConnector';
import styles from './InteractiveSearchContent.css';

const columns = [
  {
    name: 'protocol',
    label: translate('Source'),
    isSortable: true,
    isVisible: true
  },
  {
    name: 'age',
    label: translate('Age'),
    isSortable: true,
    isVisible: true
  },
  {
    name: 'releaseWeight',
    label: React.createElement(Icon, { name: icons.DOWNLOAD }),
    isSortable: true,
    fixedSortDirection: sortDirections.ASCENDING,
    isVisible: true
  },
  {
    name: 'rejections',
    label: React.createElement(Icon, { name: icons.DANGER }),
    isSortable: true,
    fixedSortDirection: sortDirections.ASCENDING,
    isVisible: true
  },
  {
    name: 'title',
    label: translate('Title'),
    isSortable: true,
    isVisible: true
  },
  {
    name: 'indexer',
    label: translate('Indexer'),
    isSortable: true,
    isVisible: true
  },
  {
    name: 'history',
    label: translate('History'),
    isSortable: true,
    fixedSortDirection: sortDirections.ASCENDING,
    isVisible: true
  },
  {
    name: 'size',
    label: translate('Size'),
    isSortable: true,
    isVisible: true
  },
  {
    name: 'peers',
    label: translate('Peers'),
    isSortable: true,
    isVisible: true
  },
  {
    name: 'languages',
    label: translate('Language'),
    isSortable: true,
    isVisible: true
  },
  {
    name: 'qualityWeight',
    label: translate('Quality'),
    isSortable: true,
    isVisible: true
  },
  {
    name: 'customFormat',
    label: translate('Formats'),
    isSortable: true,
    isVisible: true
  },
  {
    name: 'customFormatScore',
    label: React.createElement(Icon, {
      name: icons.SCORE,
      title: translate('CustomFormatScore')
    }),
    isSortable: true,
    isVisible: true
  },
  {
    name: 'indexerFlags',
    label: React.createElement(Icon, { name: icons.FLAG }),
    isSortable: true,
    isVisible: true
  }
];

function InteractiveSearchContent(props) {
  const {
    searchPayload,
    isFetching,
    isPopulated,
    error,
    totalReleasesCount,
    items,
    sortKey,
    sortDirection,
    longDateFormat,
    timeFormat,
    onSortPress,
    onGrabPress
  } = props;

  return (
    <div>
      {
        isFetching &&
          <LoadingIndicator />
      }

      {
        !isFetching && !!error &&
          <div>
            {translate('UnableToLoadResultsIntSearch')}
          </div>
      }

      {
        !isFetching && isPopulated && !totalReleasesCount &&
          <div>
            {translate('NoResultsFound')}
          </div>
      }

      {
        !!totalReleasesCount && isPopulated && !items.length &&
          <div>
            {translate('AllResultsHiddenFilter')}
          </div>
      }

      {
        isPopulated && !!items.length &&
          <Table
            columns={columns}
            sortKey={sortKey}
            sortDirection={sortDirection}
            onSortPress={onSortPress}
          >
            <TableBody>
              {
                items.map((item) => {
                  return (
                    <InteractiveSearchRowConnector
                      key={item.guid}
                      {...item}
                      searchPayload={searchPayload}
                      longDateFormat={longDateFormat}
                      timeFormat={timeFormat}
                      onGrabPress={onGrabPress}
                    />
                  );
                })
              }
            </TableBody>
          </Table>
      }

      {
        totalReleasesCount !== items.length && !!items.length &&
          <div className={styles.filteredMessage}>
            {translate('SomeResultsHiddenFilter')}
          </div>
      }
    </div>
  );
}

InteractiveSearchContent.propTypes = {
  searchPayload: PropTypes.object.isRequired,
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  totalReleasesCount: PropTypes.number.isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  sortKey: PropTypes.string,
  sortDirection: PropTypes.string,
  longDateFormat: PropTypes.string.isRequired,
  timeFormat: PropTypes.string.isRequired,
  onSortPress: PropTypes.func.isRequired,
  onGrabPress: PropTypes.func.isRequired
};

export default InteractiveSearchContent;
