import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { align, icons, sortDirections } from 'Helpers/Props';
import Button from 'Components/Link/Button';
import Icon from 'Components/Icon';
import FilterMenu from 'Components/Menu/FilterMenu';
import PageMenuButton from 'Components/Menu/PageMenuButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import InteractiveSearchFilterModalConnector from './InteractiveSearchFilterModalConnector';
import InteractiveAlbumSearchRow from './InteractiveAlbumSearchRow';
import styles from './InteractiveAlbumSearchModalContent.css';

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
    name: 'qualityWeight',
    label: 'Quality',
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

class InteractiveAlbumSearchModalContent extends Component {

  //
  // Render

  render() {
    const {
      isFetching,
      isPopulated,
      error,
      totalReleasesCount,
      items,
      selectedFilterKey,
      filters,
      customFilters,
      sortKey,
      sortDirection,
      longDateFormat,
      timeFormat,
      onSortPress,
      onFilterSelect,
      onGrabPress,
      onModalClose
    } = this.props;

    const hasItems = !!items.length;

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          Interactive Album Search
        </ModalHeader>

        <ModalBody>
          {
            isFetching &&
              <LoadingIndicator />
          }

          {
            !isFetching && !!error &&
              <div>Unable to load releases.</div>
          }

          {
            isPopulated && !hasItems && !error &&
              <div>No results.</div>
          }

          {
            isPopulated && hasItems && !error &&
            <div>
              <div className={styles.filterMenuContainer}>
                <FilterMenu
                  alignMenu={align.RIGHT}
                  selectedFilterKey={selectedFilterKey}
                  filters={filters}
                  customFilters={customFilters}
                  onFilterSelect={onFilterSelect}
                  buttonComponent={PageMenuButton}
                  filterModalConnectorComponent={InteractiveSearchFilterModalConnector}
                />
              </div>

              {
                !!totalReleasesCount && !items.length &&
                <div>
                      All results are hidden by {filters.length > 1 ? 'filters' : 'a filter'}.
                </div>
              }

              {
                !!items.length &&
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
                          <InteractiveAlbumSearchRow
                            key={item.guid}
                            {...item}
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
                  <div>
                        Some results are hidden by {filters.length > 1 ? 'filters' : 'a filter'}.
                  </div>
              }
            </div>
          }
        </ModalBody>

        <ModalFooter>
          <Button onPress={onModalClose}>
            Close
          </Button>
        </ModalFooter>
      </ModalContent>
    );
  }
}

InteractiveAlbumSearchModalContent.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  totalReleasesCount: PropTypes.number.isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  longDateFormat: PropTypes.string.isRequired,
  timeFormat: PropTypes.string.isRequired,
  selectedFilterKey: PropTypes.string.isRequired,
  filters: PropTypes.arrayOf(PropTypes.object).isRequired,
  customFilters: PropTypes.arrayOf(PropTypes.object).isRequired,
  sortKey: PropTypes.string,
  sortDirection: PropTypes.string,
  onSortPress: PropTypes.func.isRequired,
  onFilterSelect: PropTypes.func.isRequired,
  onGrabPress: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default InteractiveAlbumSearchModalContent;
