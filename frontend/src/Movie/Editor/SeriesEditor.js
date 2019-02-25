import PropTypes from 'prop-types';
import React, { Component } from 'react';
import getSelectedIds from 'Utilities/Table/getSelectedIds';
import selectAll from 'Utilities/Table/selectAll';
import toggleSelected from 'Utilities/Table/toggleSelected';
import { align, sortDirections } from 'Helpers/Props';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBodyConnector from 'Components/Page/PageContentBodyConnector';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import FilterMenu from 'Components/Menu/FilterMenu';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import NoMovie from 'Movie/NoMovie';
import OrganizeSeriesModal from './Organize/OrganizeSeriesModal';
import SeriesEditorRowConnector from './SeriesEditorRowConnector';
import SeriesEditorFooter from './SeriesEditorFooter';
import SeriesEditorFilterModalConnector from './SeriesEditorFilterModalConnector';

function getColumns() {
  return [
    {
      name: 'status',
      isSortable: true,
      isVisible: true
    },
    {
      name: 'sortTitle',
      label: 'Title',
      isSortable: true,
      isVisible: true
    },
    {
      name: 'qualityProfileId',
      label: 'Quality Profile',
      isSortable: true,
      isVisible: true
    },
    {
      name: 'path',
      label: 'Path',
      isSortable: true,
      isVisible: true
    },
    {
      name: 'tags',
      label: 'Tags',
      isSortable: false,
      isVisible: true
    }
  ];
}

class SeriesEditor extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      allSelected: false,
      allUnselected: false,
      lastToggled: null,
      selectedState: {},
      isOrganizingSeriesModalOpen: false,
      columns: getColumns()
    };
  }

  componentDidUpdate(prevProps) {
    const {
      isDeleting,
      deleteError
    } = this.props;

    const hasFinishedDeleting = prevProps.isDeleting &&
                                !isDeleting &&
                                !deleteError;

    if (hasFinishedDeleting) {
      this.onSelectAllChange({ value: false });
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

  onSaveSelected = (changes) => {
    this.props.onSaveSelected({
      seriesIds: this.getSelectedIds(),
      ...changes
    });
  }

  onOrganizeSeriesPress = () => {
    this.setState({ isOrganizingSeriesModalOpen: true });
  }

  onOrganizeSeriesModalClose = (organized) => {
    this.setState({ isOrganizingSeriesModalOpen: false });

    if (organized === true) {
      this.onSelectAllChange({ value: false });
    }
  }

  //
  // Render

  render() {
    const {
      isFetching,
      isPopulated,
      error,
      totalItems,
      items,
      selectedFilterKey,
      filters,
      customFilters,
      sortKey,
      sortDirection,
      isSaving,
      saveError,
      isDeleting,
      deleteError,
      isOrganizingSeries,
      onSortPress,
      onFilterSelect
    } = this.props;

    const {
      allSelected,
      allUnselected,
      selectedState,
      columns
    } = this.state;

    const selectedMovieIds = this.getSelectedIds();

    return (
      <PageContent title="Series Editor">
        <PageToolbar>
          <PageToolbarSection />
          <PageToolbarSection alignContent={align.RIGHT}>
            <FilterMenu
              alignMenu={align.RIGHT}
              selectedFilterKey={selectedFilterKey}
              filters={filters}
              customFilters={customFilters}
              filterModalConnectorComponent={SeriesEditorFilterModalConnector}
              onFilterSelect={onFilterSelect}
            />
          </PageToolbarSection>
        </PageToolbar>

        <PageContentBodyConnector>
          {
            isFetching && !isPopulated &&
              <LoadingIndicator />
          }

          {
            !isFetching && !!error &&
              <div>Unable to load the calendar</div>
          }

          {
            !error && isPopulated && !!items.length &&
              <div>
                <Table
                  columns={columns}
                  sortKey={sortKey}
                  sortDirection={sortDirection}
                  selectAll={true}
                  allSelected={allSelected}
                  allUnselected={allUnselected}
                  onSortPress={onSortPress}
                  onSelectAllChange={this.onSelectAllChange}
                >
                  <TableBody>
                    {
                      items.map((item) => {
                        return (
                          <SeriesEditorRowConnector
                            key={item.id}
                            {...item}
                            columns={columns}
                            isSelected={selectedState[item.id]}
                            onSelectedChange={this.onSelectedChange}
                          />
                        );
                      })
                    }
                  </TableBody>
                </Table>
              </div>
          }

          {
            !error && isPopulated && !items.length &&
              <NoMovie totalItems={totalItems} />
          }
        </PageContentBodyConnector>

        <SeriesEditorFooter
          seriesIds={selectedMovieIds}
          selectedCount={selectedMovieIds.length}
          isSaving={isSaving}
          saveError={saveError}
          isDeleting={isDeleting}
          deleteError={deleteError}
          isOrganizingSeries={isOrganizingSeries}
          onSaveSelected={this.onSaveSelected}
          onOrganizeSeriesPress={this.onOrganizeSeriesPress}
        />

        <OrganizeSeriesModal
          isOpen={this.state.isOrganizingSeriesModalOpen}
          seriesIds={selectedMovieIds}
          onModalClose={this.onOrganizeSeriesModalClose}
        />
      </PageContent>
    );
  }
}

SeriesEditor.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  totalItems: PropTypes.number.isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  sortKey: PropTypes.string,
  sortDirection: PropTypes.oneOf(sortDirections.all),
  selectedFilterKey: PropTypes.oneOfType([PropTypes.string, PropTypes.number]).isRequired,
  filters: PropTypes.arrayOf(PropTypes.object).isRequired,
  customFilters: PropTypes.arrayOf(PropTypes.object).isRequired,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  isDeleting: PropTypes.bool.isRequired,
  deleteError: PropTypes.object,
  isOrganizingSeries: PropTypes.bool.isRequired,
  onSortPress: PropTypes.func.isRequired,
  onFilterSelect: PropTypes.func.isRequired,
  onSaveSelected: PropTypes.func.isRequired
};

export default SeriesEditor;
