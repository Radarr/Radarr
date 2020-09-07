import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBodyConnector from 'Components/Page/PageContentBodyConnector';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import TableOptionsModalWrapper from 'Components/Table/TableOptions/TableOptionsModalWrapper';
import TablePager from 'Components/Table/TablePager';
import { align, icons } from 'Helpers/Props';
import hasDifferentItems from 'Utilities/Object/hasDifferentItems';
import getSelectedIds from 'Utilities/Table/getSelectedIds';
import removeOldSelectedState from 'Utilities/Table/removeOldSelectedState';
import selectAll from 'Utilities/Table/selectAll';
import toggleSelected from 'Utilities/Table/toggleSelected';
import QueueOptionsConnector from './QueueOptionsConnector';
import QueueRowConnector from './QueueRowConnector';
import RemoveQueueItemsModal from './RemoveQueueItemsModal';

class Queue extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      allSelected: false,
      allUnselected: false,
      lastToggled: null,
      selectedState: {},
      isPendingSelected: false,
      isConfirmRemoveModalOpen: false
    };
  }

  shouldComponentUpdate(nextProps) {
    // Don't update when fetching has completed if items have changed,
    // before books start fetching or when books start fetching.

    if (
      this.props.isFetching &&
      nextProps.isPopulated &&
      hasDifferentItems(this.props.items, nextProps.items) &&
      nextProps.items.some((e) => e.bookId)
    ) {
      return false;
    }

    if (!this.props.isBooksFetching && nextProps.isBooksFetching) {
      return false;
    }

    return true;
  }

  componentDidUpdate(prevProps) {
    if (hasDifferentItems(prevProps.items, this.props.items)) {
      this.setState((state) => {
        return removeOldSelectedState(state, prevProps.items);
      });

      return;
    }

    const selectedIds = this.getSelectedIds();
    const isPendingSelected = _.some(this.props.items, (item) => {
      return selectedIds.indexOf(item.id) > -1 && item.status === 'delay';
    });

    if (isPendingSelected !== this.state.isPendingSelected) {
      this.setState({ isPendingSelected });
    }
  }

  //
  // Control

  getSelectedIds = () => {
    return getSelectedIds(this.state.selectedState);
  }

  //
  // Listeners

  onSelectAllChange = ({ value }) => {
    this.setState(selectAll(this.state.selectedState, value));
  }

  onSelectedChange = ({ id, value, shiftKey = false }) => {
    this.setState((state) => {
      return toggleSelected(state, this.props.items, id, value, shiftKey);
    });
  }

  onGrabSelectedPress = () => {
    this.props.onGrabSelectedPress(this.getSelectedIds());
  }

  onRemoveSelectedPress = () => {
    this.setState({ isConfirmRemoveModalOpen: true });
  }

  onRemoveSelectedConfirmed = (payload) => {
    this.props.onRemoveSelectedPress({ ids: this.getSelectedIds(), ...payload });
    this.setState({ isConfirmRemoveModalOpen: false });
  }

  onConfirmRemoveModalClose = () => {
    this.setState({ isConfirmRemoveModalOpen: false });
  }

  //
  // Render

  render() {
    const {
      isFetching,
      isPopulated,
      error,
      items,
      isAuthorFetching,
      isAuthorPopulated,
      isBooksFetching,
      isBooksPopulated,
      booksError,
      columns,
      totalRecords,
      isGrabbing,
      isRemoving,
      isRefreshMonitoredDownloadsExecuting,
      onRefreshPress,
      ...otherProps
    } = this.props;

    const {
      allSelected,
      allUnselected,
      selectedState,
      isConfirmRemoveModalOpen,
      isPendingSelected
    } = this.state;

    const isRefreshing = isFetching || isAuthorFetching || isBooksFetching || isRefreshMonitoredDownloadsExecuting;
    const isAllPopulated = isPopulated && ((isAuthorPopulated && isBooksPopulated) || !items.length || items.every((e) => !e.bookId));
    const hasError = error || booksError;
    const selectedIds = this.getSelectedIds();
    const selectedCount = selectedIds.length;
    const disableSelectedActions = selectedCount === 0;

    return (
      <PageContent title="Queue">
        <PageToolbar>
          <PageToolbarSection>
            <PageToolbarButton
              label="Refresh"
              iconName={icons.REFRESH}
              isSpinning={isRefreshing}
              onPress={onRefreshPress}
            />

            <PageToolbarSeparator />

            <PageToolbarButton
              label="Grab Selected"
              iconName={icons.DOWNLOAD}
              isDisabled={disableSelectedActions || !isPendingSelected}
              isSpinning={isGrabbing}
              onPress={this.onGrabSelectedPress}
            />

            <PageToolbarButton
              label="Remove Selected"
              iconName={icons.REMOVE}
              isDisabled={disableSelectedActions}
              isSpinning={isRemoving}
              onPress={this.onRemoveSelectedPress}
            />
          </PageToolbarSection>

          <PageToolbarSection
            alignContent={align.RIGHT}
          >
            <TableOptionsModalWrapper
              columns={columns}
              {...otherProps}
              optionsComponent={QueueOptionsConnector}
            >
              <PageToolbarButton
                label="Options"
                iconName={icons.TABLE}
              />
            </TableOptionsModalWrapper>
          </PageToolbarSection>
        </PageToolbar>

        <PageContentBodyConnector>
          {
            isRefreshing && !isAllPopulated &&
              <LoadingIndicator />
          }

          {
            !isRefreshing && hasError &&
              <div>
                Failed to load Queue
              </div>
          }

          {
            isPopulated && !hasError && !items.length &&
              <div>
                Queue is empty
              </div>
          }

          {
            isAllPopulated && !hasError && !!items.length &&
              <div>
                <Table
                  columns={columns}
                  selectAll={true}
                  allSelected={allSelected}
                  allUnselected={allUnselected}
                  {...otherProps}
                  optionsComponent={QueueOptionsConnector}
                  onSelectAllChange={this.onSelectAllChange}
                >
                  <TableBody>
                    {
                      items.map((item) => {
                        return (
                          <QueueRowConnector
                            key={item.id}
                            bookId={item.bookId}
                            isSelected={selectedState[item.id]}
                            columns={columns}
                            {...item}
                            onSelectedChange={this.onSelectedChange}
                          />
                        );
                      })
                    }
                  </TableBody>
                </Table>

                <TablePager
                  totalRecords={totalRecords}
                  isFetching={isRefreshing}
                  {...otherProps}
                />
              </div>
          }
        </PageContentBodyConnector>

        <RemoveQueueItemsModal
          isOpen={isConfirmRemoveModalOpen}
          selectedCount={selectedCount}
          canIgnore={isConfirmRemoveModalOpen && (
            selectedIds.every((id) => {
              const item = items.find((i) => i.id === id);

              return !!(item && item.authorId && item.bookId);
            })
          )}
          onRemovePress={this.onRemoveSelectedConfirmed}
          onModalClose={this.onConfirmRemoveModalClose}
        />
      </PageContent>
    );
  }
}

Queue.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  isAuthorFetching: PropTypes.bool.isRequired,
  isAuthorPopulated: PropTypes.bool.isRequired,
  isBooksFetching: PropTypes.bool.isRequired,
  isBooksPopulated: PropTypes.bool.isRequired,
  booksError: PropTypes.object,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  totalRecords: PropTypes.number,
  isGrabbing: PropTypes.bool.isRequired,
  isRemoving: PropTypes.bool.isRequired,
  isRefreshMonitoredDownloadsExecuting: PropTypes.bool.isRequired,
  onRefreshPress: PropTypes.func.isRequired,
  onGrabSelectedPress: PropTypes.func.isRequired,
  onRemoveSelectedPress: PropTypes.func.isRequired
};

export default Queue;
