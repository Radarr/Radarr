import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Alert from 'Components/Alert';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import TablePager from 'Components/Table/TablePager';
import { icons, kinds } from 'Helpers/Props';
import InteractiveImportModal from 'InteractiveImport/InteractiveImportModal';
import getFilterValue from 'Utilities/Filter/getFilterValue';
import hasDifferentItems from 'Utilities/Object/hasDifferentItems';
import removeOldSelectedState from 'Utilities/Table/removeOldSelectedState';
import selectAll from 'Utilities/Table/selectAll';
import toggleSelected from 'Utilities/Table/toggleSelected';

function getMonitoredValue(props) {
  const {
    filters,
    selectedFilterKey
  } = props;

  return getFilterValue(filters, selectedFilterKey, 'monitored', false);
}

class CutoffUnmet extends Component {

  //
  // Lifecycle

  constructor(props) {
    super(props);

    this.state = {
      allSelected: false,
      allUnselected: false,
      lastToggled: null,
      selectedState: {},
      isConfirmSearchAllMissingModalOpen: false,
      isInteractiveImportModalOpen: false
    };
  }

  componentDidUpdate(prevProps) {
    if (hasDifferentItems(prevProps.items, this.props.items)) {
      this.setState((state) => {
        return removeOldSelectedState(state, prevProps.items);
      });
    }
  }

  //
  // Control

  getSelectedIds() {
    return [];
  }

  //
  // Listeners

  onSelectAllChange = ({ value }) => {
    this.setState(selectAll(this.state.selectedState, value));
  };

  onSelectedChange = ({ id, value, shiftKey = false }) => {
    this.setState((state) => {
      return toggleSelected(state, this.props.items, id, value, shiftKey);
    });
  };

  onSearchSelectedPress = () => {
    const selected = this.getSelectedIds();

    this.props.onSearchSelectedPress(selected);
  };

  onToggleSelectedPress = () => {
    const movieIds = this.getSelectedIds();

    this.props.batchToggleMissingMovies({
      movieIds,
      monitored: !getMonitoredValue(this.props)
    });
  };

  onSearchAllMissingPress = () => {
    this.setState({ isConfirmSearchAllMissingModalOpen: true });
  };

  onSearchAllMissingConfirmed = () => {
    const {
      selectedFilterKey,
      onSearchAllMissingPress
    } = this.props;

    // TODO: Custom filters will need to check whether there is a monitored
    // filter once implemented.

    onSearchAllMissingPress(selectedFilterKey === 'monitored');
    this.setState({ isConfirmSearchAllMissingModalOpen: false });
  };

  onConfirmSearchAllMissingModalClose = () => {
    this.setState({ isConfirmSearchAllMissingModalOpen: false });
  };

  onInteractiveImportPress = () => {
    this.setState({ isInteractiveImportModalOpen: true });
  };

  onInteractiveImportModalClose = () => {
    this.setState({ isInteractiveImportModalOpen: false });
  };

  //
  // Render

  render() {
    const {
      isFetching,
      isPopulated,
      error,
      items,
      selectedFilterKey,
      filters,
      columns,
      totalRecords,
      isSearchingForMissingMovies,
      isSaving,
      onFilterSelect,
      ...otherProps
    } = this.props;

    const {
      allSelected,
      allUnselected,
      selectedState,
      isConfirmSearchAllMissingModalOpen,
      isInteractiveImportModalOpen
    } = this.state;

    const itemsSelected = !!this.getSelectedIds().length;
    const isShowingMonitored = getMonitoredValue(this.props);

    return (
      <PageContent title="Missing">
        <PageToolbar>
          <PageToolbarSection>
            <PageToolbarButton
              label="Search Selected"
              iconName={icons.SEARCH}
              isDisabled={!itemsSelected || isSearchingForMissingMovies}
              onPress={this.onSearchSelectedPress}
            />

            <PageToolbarButton
              label={isShowingMonitored ? 'Unmonitor Selected' : 'Monitor Selected'}
              iconName={icons.MONITORED}
              isDisabled={!itemsSelected}
              isSpinning={isSaving}
              onPress={this.onToggleSelectedPress}
            />

            <PageToolbarSeparator />

            <PageToolbarButton
              label="Search All"
              iconName={icons.SEARCH}
              isDisabled={!items.length}
              isSpinning={isSearchingForMissingMovies}
              onPress={this.onSearchAllMissingPress}
            />

          </PageToolbarSection>
        </PageToolbar>

        <PageContentBody>
          {
            isFetching && !isPopulated &&
              <LoadingIndicator />
          }

          {
            !isFetching && error &&
              <Alert kind={kinds.DANGER}>
                Error fetching missing items
              </Alert>
          }

          {
            isPopulated && !error && !items.length &&
              <Alert kind={kinds.INFO}>
                No missing items
              </Alert>
          }

          {
            isPopulated && !error && !!items.length &&
              <div>
                <Table
                  columns={columns}
                  selectAll={true}
                  allSelected={allSelected}
                  allUnselected={allUnselected}
                  {...otherProps}
                  onSelectAllChange={this.onSelectAllChange}
                >
                  <TableBody>
                    {
                      items.map((item) => {
                        return (
                          <></>
                        );
                      })
                    }
                  </TableBody>
                </Table>

                <TablePager
                  totalRecords={totalRecords}
                  isFetching={isFetching}
                  {...otherProps}
                />

                <ConfirmModal
                  isOpen={isConfirmSearchAllMissingModalOpen}
                  kind={kinds.DANGER}
                  title="Search for all missing episodes"
                  message={
                    <div>
                      <div>
                        Are you sure you want to search for all {totalRecords} missing movies?
                      </div>
                      <div>
                        This cannot be cancelled once started without restarting Sonarr or disabling all of your indexers.
                      </div>
                    </div>
                  }
                  confirmLabel="Search"
                  onConfirm={this.onSearchAllMissingConfirmed}
                  onCancel={this.onConfirmSearchAllMissingModalClose}
                />
              </div>
          }

          <InteractiveImportModal
            isOpen={isInteractiveImportModalOpen}
            onModalClose={this.onInteractiveImportModalClose}
          />
        </PageContentBody>
      </PageContent>
    );
  }
}

CutoffUnmet.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  selectedFilterKey: PropTypes.string.isRequired,
  filters: PropTypes.arrayOf(PropTypes.object).isRequired,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  totalRecords: PropTypes.number,
  isSearchingForMissingMovies: PropTypes.bool.isRequired,
  isSaving: PropTypes.bool.isRequired,
  onFilterSelect: PropTypes.func.isRequired,
  onSearchSelectedPress: PropTypes.func.isRequired,
  batchToggleMissingMovies: PropTypes.func.isRequired,
  onSearchAllMissingPress: PropTypes.func.isRequired
};

export default CutoffUnmet;
