import PropTypes from 'prop-types';
import React, { Fragment } from 'react';
import Alert from 'Components/Alert';
import Icon from 'Components/Icon';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import { icons, kinds, sortDirections } from 'Helpers/Props';
import getErrorMessage from 'Utilities/Object/getErrorMessage';
import translate from 'Utilities/String/translate';
import InteractiveSearchRowConnector from './InteractiveSearchRowConnector';
import styles from './InteractiveSearchContent.css';

const columns = [
  {
    name: 'protocol',
    label: () => translate('Source'),
    isSortable: true,
    isVisible: true
  },
  {
    name: 'age',
    label: () => translate('Age'),
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
    label: React.createElement(Icon, {
      name: icons.DANGER,
      title: () => translate('Rejections')
    }),
    isSortable: true,
    fixedSortDirection: sortDirections.ASCENDING,
    isVisible: true
  },
  {
    name: 'title',
    label: () => translate('Title'),
    isSortable: true,
    isVisible: true
  },
  {
    name: 'indexer',
    label: () => translate('Indexer'),
    isSortable: true,
    isVisible: true
  },
  {
    name: 'history',
    label: () => translate('History'),
    isSortable: true,
    fixedSortDirection: sortDirections.ASCENDING,
    isVisible: true
  },
  {
    name: 'size',
    label: () => translate('Size'),
    isSortable: true,
    isVisible: true
  },
  {
    name: 'peers',
    label: () => translate('Peers'),
    isSortable: true,
    isVisible: true
  },
  {
    name: 'languages',
    label: () => translate('Language'),
    isSortable: true,
    isVisible: true
  },
  {
    name: 'qualityWeight',
    label: () => translate('Quality'),
    isSortable: true,
    isVisible: true
  },
  {
    name: 'customFormat',
    label: () => translate('Formats'),
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
    label: React.createElement(Icon, {
      name: icons.FLAG,
      title: () => translate('IndexerFlags')
    }),
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

  const errorMessage = getErrorMessage(error);

  return (
    <div>
      {
        isFetching ? <LoadingIndicator /> : null
      }

      {
        !isFetching && error ?
          <Alert kind={kinds.DANGER} className={styles.alert}>
            {
              errorMessage ?
                <Fragment>
                  {translate('InteractiveSearchResultsFailedErrorMessage', { message: errorMessage.charAt(0).toLowerCase() + errorMessage.slice(1) })}
                </Fragment> :
                translate('MovieSearchResultsLoadError')
            }
          </Alert> :
          null
      }

      {
        !isFetching && isPopulated && !totalReleasesCount ?
          <Alert kind={kinds.INFO} className={styles.alert}>
            {translate('NoResultsFound')}
          </Alert> :
          null
      }

      {
        !!totalReleasesCount && isPopulated && !items.length ?
          <Alert kind={kinds.WARNING} className={styles.alert}>
            {translate('AllResultsHiddenFilter')}
          </Alert> :
          null
      }

      {
        isPopulated && !!items.length ?
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
          </Table> :
          null
      }

      {
        totalReleasesCount !== items.length && !!items.length ?
          <Alert kind={kinds.INFO} className={styles.alert}>
            {translate('SomeResultsHiddenFilter')}
          </Alert> :
          null
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
