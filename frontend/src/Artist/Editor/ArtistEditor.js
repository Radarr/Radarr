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
import MenuContent from 'Components/Menu/MenuContent';
import FilterMenuItem from 'Components/Menu/FilterMenuItem';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import NoArtist from 'Artist/NoArtist';
import ArtistEditorRowConnector from './ArtistEditorRowConnector';
import ArtistEditorFooter from './ArtistEditorFooter';
import OrganizeArtistModal from './Organize/OrganizeArtistModal';

function getColumns(showLanguageProfile, showMetadataProfile) {
  return [
    {
      name: 'status',
      isVisible: true
    },
    {
      name: 'sortName',
      label: 'Name',
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
      name: 'languageProfileId',
      label: 'Language Profile',
      isSortable: true,
      isVisible: showLanguageProfile
    },
    {
      name: 'metadataProfileId',
      label: 'Metadata Profile',
      isSortable: true,
      isVisible: showMetadataProfile
    },
    {
      name: 'albumFolder',
      label: 'Album Folder',
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

class ArtistEditor extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      allSelected: false,
      allUnselected: false,
      lastToggled: null,
      selectedState: {},
      isOrganizingArtistModalOpen: false,
      columns: getColumns(props.showLanguageProfile, props.showMetadataProfile)
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
      artistIds: this.getSelectedIds(),
      ...changes
    });
  }

  onOrganizeArtistPress = () => {
    this.setState({ isOrganizingArtistModalOpen: true });
  }

  onOrganizeArtistModalClose = (organized) => {
    this.setState({ isOrganizingArtistModalOpen: false });

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
      items,
      filterKey,
      filterValue,
      sortKey,
      sortDirection,
      isSaving,
      saveError,
      isDeleting,
      deleteError,
      isOrganizingArtist,
      showLanguageProfile,
      showMetadataProfile,
      onSortPress,
      onFilterSelect
    } = this.props;

    const {
      allSelected,
      allUnselected,
      selectedState,
      columns
    } = this.state;

    const selectedArtistIds = this.getSelectedIds();

    return (
      <PageContent title="Artist Editor">
        <PageToolbar>
          <PageToolbarSection />
          <PageToolbarSection alignContent={align.RIGHT}>
            <FilterMenu alignMenu={align.RIGHT}>
              <MenuContent>
                <FilterMenuItem
                  filterKey={filterKey}
                  filterValue={filterValue}
                  onPress={onFilterSelect}
                >
                  All
                </FilterMenuItem>

                <FilterMenuItem
                  name="monitored"
                  value={true}
                  filterKey={filterKey}
                  filterValue={filterValue}
                  onPress={onFilterSelect}
                >
                  Monitored Only
                </FilterMenuItem>

                <FilterMenuItem
                  name="status"
                  value="continuing"
                  filterKey={filterKey}
                  filterValue={filterValue}
                  onPress={onFilterSelect}
                >
                  Continuing Only
                </FilterMenuItem>

                <FilterMenuItem
                  name="status"
                  value="ended"
                  filterKey={filterKey}
                  filterValue={filterValue}
                  onPress={onFilterSelect}
                >
                  Ended Only
                </FilterMenuItem>

                <FilterMenuItem
                  name="missing"
                  value={true}
                  filterKey={filterKey}
                  filterValue={filterValue}
                  onPress={onFilterSelect}
                >
                  Missing Albums
                </FilterMenuItem>
              </MenuContent>
            </FilterMenu>
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
                          <ArtistEditorRowConnector
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
              <NoArtist />
          }
        </PageContentBodyConnector>

        <ArtistEditorFooter
          artistIds={selectedArtistIds}
          selectedCount={selectedArtistIds.length}
          isSaving={isSaving}
          saveError={saveError}
          isDeleting={isDeleting}
          deleteError={deleteError}
          isOrganizingArtist={isOrganizingArtist}
          showLanguageProfile={showLanguageProfile}
          showMetadataProfile={showMetadataProfile}
          onSaveSelected={this.onSaveSelected}
          onOrganizeArtistPress={this.onOrganizeArtistPress}
        />

        <OrganizeArtistModal
          isOpen={this.state.isOrganizingArtistModalOpen}
          artistIds={selectedArtistIds}
          onModalClose={this.onOrganizeArtistModalClose}
        />
      </PageContent>
    );
  }
}

ArtistEditor.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  sortKey: PropTypes.string,
  sortDirection: PropTypes.oneOf(sortDirections.all),
  filterKey: PropTypes.string,
  filterValue: PropTypes.oneOfType([PropTypes.bool, PropTypes.number, PropTypes.string]),
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  isDeleting: PropTypes.bool.isRequired,
  deleteError: PropTypes.object,
  isOrganizingArtist: PropTypes.bool.isRequired,
  showLanguageProfile: PropTypes.bool.isRequired,
  showMetadataProfile: PropTypes.bool.isRequired,
  onSortPress: PropTypes.func.isRequired,
  onFilterSelect: PropTypes.func.isRequired,
  onSaveSelected: PropTypes.func.isRequired
};

export default ArtistEditor;
