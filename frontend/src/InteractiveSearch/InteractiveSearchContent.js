import PropTypes from 'prop-types';
import React from 'react';
import Alert from 'Components/Alert';
import Icon from 'Components/Icon';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import { icons, kinds, sortDirections } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import InteractiveSearchRowConnector from './InteractiveSearchRowConnector';
import styles from './InteractiveSearchContent.css';

const columns = [
  {
    name: 'protocol',
    get label() {
      return translate('Source');
    },
    isSortable: true,
    isVisible: true
  },
  {
    name: 'age',
    get label() {
      return translate('Age');
    },
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
    get label() {
      return translate('Title');
    },
    isSortable: true,
    isVisible: true
  },
  {
    name: 'indexer',
    get label() {
      return translate('Indexer');
    },
    isSortable: true,
    isVisible: true
  },
  {
    name: 'history',
    get label() {
      return translate('History');
    },
    isSortable: true,
    fixedSortDirection: sortDirections.ASCENDING,
    isVisible: true
  },
  {
    name: 'size',
    get label() {
      return translate('Size');
    },
    isSortable: true,
    isVisible: true
  },
  {
    name: 'peers',
    get label() {
      return translate('Peers');
    },
    isSortable: true,
    isVisible: true
  },
  {
    name: 'languages',
    get label() {
      return translate('Language');
    },
    isSortable: true,
    isVisible: true
  },
  {
    name: 'qualityWeight',
    get label() {
      return translate('Quality');
    },
    isSortable: true,
    isVisible: true
  },
  {
    name: 'customFormat',
    get label() {
      return translate('Formats');
    },
    isSortable: true,
    isVisible: true
  },
  {
    name: 'customFormatScore',
    label: React.createElement(Icon, {
      name: icons.SCORE,
      title: () => translate('CustomFormatScore')
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
          <Alert kind={kinds.DANGER} className={styles.alert}>
            {translate('UnableToLoadResultsIntSearch')}
          </Alert>
      }

      {
        !isFetching && isPopulated && !totalReleasesCount &&
          <Alert kind={kinds.INFO} className={styles.alert}>
            {translate('NoResultsFound')}
          </Alert>
      }

      {
        !!totalReleasesCount && isPopulated && !items.length &&
          <Alert kind={kinds.WARNING} className={styles.alert}>
            {translate('AllResultsHiddenFilter')}
          </Alert>
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
                      key={`${item.indexerId}-${item.guid}`}
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
          <Alert kind={kinds.INFO} className={styles.alert}>
            {translate('SomeResultsHiddenFilter')}
          </Alert>
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
