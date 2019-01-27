import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import getErrorMessage from 'Utilities/Object/getErrorMessage';
import getSelectedIds from 'Utilities/Table/getSelectedIds';
import selectAll from 'Utilities/Table/selectAll';
import toggleSelected from 'Utilities/Table/toggleSelected';
import { align, icons, kinds } from 'Helpers/Props';
import Button from 'Components/Link/Button';
import Icon from 'Components/Icon';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import SelectInput from 'Components/Form/SelectInput';
import Menu from 'Components/Menu/Menu';
import MenuButton from 'Components/Menu/MenuButton';
import MenuContent from 'Components/Menu/MenuContent';
import SelectedMenuItem from 'Components/Menu/SelectedMenuItem';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import SelectArtistModal from 'InteractiveImport/Artist/SelectArtistModal';
import SelectAlbumModal from 'InteractiveImport/Album/SelectAlbumModal';
import InteractiveImportRow from './InteractiveImportRow';
import styles from './InteractiveImportModalContent.css';

const columns = [
  {
    name: 'relativePath',
    label: 'Relative Path',
    isSortable: true,
    isVisible: true
  },
  {
    name: 'artist',
    label: 'Artist',
    isSortable: true,
    isVisible: true
  },
  {
    name: 'album',
    label: 'Album',
    isVisible: true
  },
  {
    name: 'tracks',
    label: 'Track(s)',
    isVisible: true
  },
  {
    name: 'quality',
    label: 'Quality',
    isSortable: true,
    isVisible: true
  },
  {
    name: 'language',
    label: 'Language',
    isSortable: true,
    isVisible: true
  },
  {
    name: 'size',
    label: 'Size',
    isVisible: true
  },
  {
    name: 'rejections',
    label: React.createElement(Icon, {
      name: icons.DANGER,
      kind: kinds.DANGER
    }),
    isVisible: true
  }
];

const filterExistingFilesOptions = {
  ALL: 'all',
  NEW: 'new'
};

class InteractiveImportModalContent extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      allSelected: false,
      allUnselected: false,
      lastToggled: null,
      selectedState: {},
      invalidRowsSelected: [],
      isSelectArtistModalOpen: false,
      isSelectAlbumModalOpen: false
    };
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

  onValidRowChange = (id, isValid) => {
    this.setState((state) => {
      if (isValid) {
        return {
          invalidRowsSelected: _.without(state.invalidRowsSelected, id)
        };
      }

      return {
        invalidRowsSelected: [...state.invalidRowsSelected, id]
      };
    });
  }

  onImportSelectedPress = () => {
    const selected = this.getSelectedIds();

    this.props.onImportSelectedPress(selected, this.props.importMode);
  }

  onFilterExistingFilesChange = (value) => {
    this.props.onFilterExistingFilesChange(value !== filterExistingFilesOptions.ALL);
  }

  onImportModeChange = ({ value }) => {
    this.props.onImportModeChange(value);
  }

  onSelectArtistPress = () => {
    this.setState({ isSelectArtistModalOpen: true });
  }

  onSelectAlbumPress = () => {
    this.setState({ isSelectAlbumModalOpen: true });
  }

  onSelectArtistModalClose = () => {
    this.setState({ isSelectArtistModalOpen: false });
  }

  onSelectAlbumModalClose = () => {
    this.setState({ isSelectAlbumModalOpen: false });
  }

  //
  // Render

  render() {
    const {
      downloadId,
      allowArtistChange,
      showFilterExistingFiles,
      showImportMode,
      filterExistingFiles,
      title,
      folder,
      isFetching,
      isPopulated,
      error,
      items,
      sortKey,
      sortDirection,
      importMode,
      interactiveImportErrorMessage,
      onSortPress,
      onModalClose
    } = this.props;

    const {
      allSelected,
      allUnselected,
      selectedState,
      invalidRowsSelected,
      isSelectArtistModalOpen,
      isSelectAlbumModalOpen
    } = this.state;

    const selectedIds = this.getSelectedIds();
    const selectedItem = selectedIds.length ? _.find(items, { id: selectedIds[0] }) : null;
    const errorMessage = getErrorMessage(error, 'Unable to load manual import items');

    const importModeOptions = [
      { key: 'move', value: 'Move Files' },
      { key: 'copy', value: 'Copy Files' }
    ];

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          Manual Import - {title || folder}
        </ModalHeader>

        <ModalBody>
          {
            showFilterExistingFiles &&
            <div className={styles.filterContainer}>
              <Menu alignMenu={align.RIGHT}>
                <MenuButton>
                  <Icon
                    name={icons.FILTER}
                    size={22}
                  />

                  <div className={styles.filterText}>
                    {
                      filterExistingFiles ? 'Unmapped Files Only' : 'All Files'
                    }
                  </div>
                </MenuButton>

                <MenuContent>
                  <SelectedMenuItem
                    name={filterExistingFilesOptions.ALL}
                    isSelected={!filterExistingFiles}
                    onPress={this.onFilterExistingFilesChange}
                  >
                    All Files
                  </SelectedMenuItem>

                  <SelectedMenuItem
                    name={filterExistingFilesOptions.NEW}
                    isSelected={filterExistingFiles}
                    onPress={this.onFilterExistingFilesChange}
                  >
                    Unmapped Files Only
                  </SelectedMenuItem>
                </MenuContent>
              </Menu>
            </div>
          }

          {
            isFetching &&
              <LoadingIndicator />
          }

          {
            error &&
              <div>{errorMessage}</div>
          }

          {
            isPopulated && !!items.length && !isFetching && !isFetching &&
              <Table
                columns={columns}
                selectAll={true}
                allSelected={allSelected}
                allUnselected={allUnselected}
                sortKey={sortKey}
                sortDirection={sortDirection}
                onSortPress={onSortPress}
                onSelectAllChange={this.onSelectAllChange}
              >
                <TableBody>
                  {
                    items.map((item) => {
                      return (
                        <InteractiveImportRow
                          key={item.id}
                          isSelected={selectedState[item.id]}
                          {...item}
                          allowArtistChange={allowArtistChange}
                          onSelectedChange={this.onSelectedChange}
                          onValidRowChange={this.onValidRowChange}
                        />
                      );
                    })
                  }
                </TableBody>
              </Table>
          }

          {
            isPopulated && !items.length && !isFetching &&
              'No audio files were found in the selected folder'
          }
        </ModalBody>

        <ModalFooter className={styles.footer}>
          <div className={styles.leftButtons}>
            {
              !downloadId && showImportMode &&
                <SelectInput
                  className={styles.importMode}
                  name="importMode"
                  value={importMode}
                  values={importModeOptions}
                  onChange={this.onImportModeChange}
                />
            }
          </div>

          <div className={styles.centerButtons}>
            {
              allowArtistChange &&
                <Button
                  onPress={this.onSelectArtistPress}
                  isDisabled={!selectedIds.length}
                >
                  Select Artist
                </Button>
            }

            <Button
              onPress={this.onSelectAlbumPress}
              isDisabled={!selectedIds.length}
            >
              Select Album
            </Button>
          </div>

          <div className={styles.rightButtons}>
            <Button onPress={onModalClose}>
              Cancel
            </Button>

            {
              interactiveImportErrorMessage &&
                <span className={styles.errorMessage}>{interactiveImportErrorMessage}</span>
            }

            <Button
              kind={kinds.SUCCESS}
              isDisabled={!selectedIds.length || !!invalidRowsSelected.length}
              onPress={this.onImportSelectedPress}
            >
              Import
            </Button>
          </div>
        </ModalFooter>

        <SelectArtistModal
          isOpen={isSelectArtistModalOpen}
          ids={selectedIds}
          onModalClose={this.onSelectArtistModalClose}
        />

        <SelectAlbumModal
          isOpen={isSelectAlbumModalOpen}
          ids={selectedIds}
          artistId={selectedItem && selectedItem.artist && selectedItem.artist.id}
          onModalClose={this.onSelectAlbumModalClose}
        />
      </ModalContent>
    );
  }
}

InteractiveImportModalContent.propTypes = {
  downloadId: PropTypes.string,
  allowArtistChange: PropTypes.bool.isRequired,
  showImportMode: PropTypes.bool.isRequired,
  showFilterExistingFiles: PropTypes.bool.isRequired,
  filterExistingFiles: PropTypes.bool.isRequired,
  importMode: PropTypes.string.isRequired,
  title: PropTypes.string,
  folder: PropTypes.string,
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  sortKey: PropTypes.string,
  sortDirection: PropTypes.string,
  interactiveImportErrorMessage: PropTypes.string,
  onSortPress: PropTypes.func.isRequired,
  onFilterExistingFilesChange: PropTypes.func.isRequired,
  onImportModeChange: PropTypes.func.isRequired,
  onImportSelectedPress: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

InteractiveImportModalContent.defaultProps = {
  allowArtistChange: true,
  showFilterExistingFiles: false,
  showImportMode: true,
  importMode: 'move'
};

export default InteractiveImportModalContent;
