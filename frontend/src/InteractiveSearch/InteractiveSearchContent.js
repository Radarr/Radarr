import PropTypes from 'prop-types';
import React from 'react';
import { icons, sortDirections } from 'Helpers/Props';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import Icon from 'Components/Icon';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import InteractiveSearchRow from './InteractiveSearchRow';
import styles from './InteractiveSearchContent.css';

const columns = [
  {
    name: 'protocol',
    label: 'Source',
    isSortable: true,
    isVisible: true
  },
  {
    name: 'age',
    label: 'Age',
    isSortable: true,
    isVisible: true
  },
  {
    name: 'title',
    label: 'Title',
    isSortable: true,
    isVisible: true
  },
  {
    name: 'indexer',
    label: 'Indexer',
    isSortable: true,
    isVisible: true
  },
  {
    name: 'size',
    label: 'Size',
    isSortable: true,
    isVisible: true
  },
  {
    name: 'peers',
    label: 'Peers',
    isSortable: true,
    isVisible: true
  },
  {
    name: 'languageWeight',
    label: 'Language',
    isSortable: true,
    isVisible: true
  },
  {
    name: 'qualityWeight',
    label: 'Quality',
    isSortable: true,
    isVisible: true
  },
  {
    name: 'customFormat',
    label: 'Formats',
    isSortable: true,
    isVisible: true
  },
  {
    name: 'customFormatScore',
    label: React.createElement(Icon, {
      name: icons.SCORE,
      title: 'Custom Format score'
    }),
    isSortable: true,
    isVisible: true
  },
  {
    name: 'indexerFlags',
    label: React.createElement(Icon, { name: icons.FLAG }),
    isSortable: true,
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
    name: 'releaseWeight',
    label: React.createElement(Icon, { name: icons.DOWNLOAD }),
    isSortable: true,
    fixedSortDirection: sortDirections.ASCENDING,
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
            Unable to load results for this movie search. Try again later
          </div>
      }

      {
        !isFetching && isPopulated && !totalReleasesCount &&
          <div>
            No results found
          </div>
      }

      {
        !!totalReleasesCount && isPopulated && !items.length &&
          <div>
            All results are hidden by the applied filter
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
                    <InteractiveSearchRow
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
                Some results are hidden by the applied filter
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
