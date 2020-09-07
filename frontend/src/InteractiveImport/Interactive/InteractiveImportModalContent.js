import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import SelectInput from 'Components/Form/SelectInput';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import Menu from 'Components/Menu/Menu';
import MenuButton from 'Components/Menu/MenuButton';
import MenuContent from 'Components/Menu/MenuContent';
import SelectedMenuItem from 'Components/Menu/SelectedMenuItem';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import { align, icons, kinds, scrollDirections } from 'Helpers/Props';
import SelectAuthorModal from 'InteractiveImport/Author/SelectAuthorModal';
import SelectBookModal from 'InteractiveImport/Book/SelectBookModal';
import ConfirmImportModal from 'InteractiveImport/Confirmation/ConfirmImportModal';
import SelectEditionModal from 'InteractiveImport/Edition/SelectEditionModal';
import SelectQualityModal from 'InteractiveImport/Quality/SelectQualityModal';
import getErrorMessage from 'Utilities/Object/getErrorMessage';
import getSelectedIds from 'Utilities/Table/getSelectedIds';
import selectAll from 'Utilities/Table/selectAll';
import toggleSelected from 'Utilities/Table/toggleSelected';
import InteractiveImportRow from './InteractiveImportRow';
import styles from './InteractiveImportModalContent.css';

const columns = [
  {
    name: 'path',
    label: 'Path',
    isSortable: true,
    isVisible: true
  },
  {
    name: 'author',
    label: 'Author',
    isSortable: true,
    isVisible: true
  },
  {
    name: 'book',
    label: 'Book',
    isVisible: true
  },
  {
    name: 'quality',
    label: 'Quality',
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

const importModeOptions = [
  { key: 'move', value: 'Move Files' },
  { key: 'copy', value: 'Hardlink/Copy Files' }
];

const SELECT = 'select';
const AUTHOR = 'author';
const BOOK = 'book';
const EDITION = 'edition';
const QUALITY = 'quality';

const replaceExistingFilesOptions = {
  COMBINE: 'combine',
  DELETE: 'delete'
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
      selectModalOpen: null,
      booksImported: [],
      isConfirmImportModalOpen: false,
      inconsistentBookReleases: false
    };
  }

  componentDidUpdate(prevProps) {
    const selectedIds = this.getSelectedIds();
    const selectedItems = _.filter(this.props.items, (x) => _.includes(selectedIds, x.id));

    const inconsistent = _(selectedItems)
      .map((x) => ({ bookId: x.book ? x.book.id : 0, releaseId: x.EditionId }))
      .groupBy('bookId')
      .mapValues((book) => _(book).groupBy((x) => x.releaseId).values().value().length)
      .values()
      .some((x) => x !== undefined && x > 1);

    if (inconsistent !== this.state.inconsistentBookReleases) {
      this.setState({ inconsistentBookReleases: inconsistent });
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

  onValidRowChange = (id, isValid) => {
    this.setState((state, props) => {
      // make sure to exclude any invalidRows that are no longer present in props
      const diff = _.difference(state.invalidRowsSelected, _.map(props.items, 'id'));
      const currentInvalid = _.difference(state.invalidRowsSelected, diff);
      const newstate = isValid ? _.without(currentInvalid, id) : _.union(currentInvalid, [id]);
      return { invalidRowsSelected: newstate };
    });
  }

  onImportSelectedPress = () => {
    if (!this.props.replaceExistingFiles) {
      this.onConfirmImportPress();
      return;
    }

    // potentially deleting files
    const selectedIds = this.getSelectedIds();
    const booksImported = _(this.props.items)
      .filter((x) => _.includes(selectedIds, x.id))
      .keyBy((x) => x.book.id)
      .map((x) => x.book)
      .value();

    console.log(booksImported);

    this.setState({
      booksImported,
      isConfirmImportModalOpen: true
    });
  }

  onConfirmImportPress = () => {
    const {
      downloadId,
      showImportMode,
      importMode,
      onImportSelectedPress
    } = this.props;

    const selected = this.getSelectedIds();
    const finalImportMode = downloadId || !showImportMode ? 'auto' : importMode;

    onImportSelectedPress(selected, finalImportMode);
  }

  onFilterExistingFilesChange = (value) => {
    this.props.onFilterExistingFilesChange(value !== filterExistingFilesOptions.ALL);
  }

  onReplaceExistingFilesChange = (value) => {
    this.props.onReplaceExistingFilesChange(value === replaceExistingFilesOptions.DELETE);
  }

  onImportModeChange = ({ value }) => {
    this.props.onImportModeChange(value);
  }

  onSelectModalSelect = ({ value }) => {
    this.setState({ selectModalOpen: value });
  }

  onClearBookMappingPress = () => {
    const selectedIds = this.getSelectedIds();

    selectedIds.forEach((id) => {
      this.props.updateInteractiveImportItem({
        id,
        rejections: []
      });
    });
  }

  onGetBookMappingPress = () => {
    this.props.saveInteractiveImportItem({ id: this.getSelectedIds() });
  }

  onSelectModalClose = () => {
    this.setState({ selectModalOpen: null });
  }

  onConfirmImportModalClose = () => {
    this.setState({ isConfirmImportModalOpen: false });
  }

  //
  // Render

  render() {
    const {
      downloadId,
      allowAuthorChange,
      showFilterExistingFiles,
      showReplaceExistingFiles,
      showImportMode,
      filterExistingFiles,
      replaceExistingFiles,
      title,
      folder,
      isFetching,
      isPopulated,
      isSaving,
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
      selectModalOpen,
      booksImported,
      isConfirmImportModalOpen,
      inconsistentBookReleases
    } = this.state;

    const selectedIds = this.getSelectedIds();
    const selectedItem = selectedIds.length ? _.find(items, { id: selectedIds[0] }) : null;
    const errorMessage = getErrorMessage(error, 'Unable to load manual import items');

    const bulkSelectOptions = [
      { key: SELECT, value: 'Select...', disabled: true },
      { key: BOOK, value: 'Select Book' },
      { key: EDITION, value: 'Select Edition' },
      { key: QUALITY, value: 'Select Quality' }
    ];

    if (allowAuthorChange) {
      bulkSelectOptions.splice(1, 0, {
        key: AUTHOR,
        value: 'Select Author'
      });
    }

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          Manual Import - {title || folder}
        </ModalHeader>

        <ModalBody scrollDirection={scrollDirections.BOTH}>
          <div className={styles.filterContainer}>
            {
              showFilterExistingFiles &&
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
            }
            {
              showReplaceExistingFiles &&
                <Menu alignMenu={align.RIGHT}>
                  <MenuButton>
                    <Icon
                      name={icons.CLONE}
                      size={22}
                    />

                    <div className={styles.filterText}>
                      {
                        replaceExistingFiles ? 'Existing files will be deleted' : 'Combine with existing files'
                      }
                    </div>
                  </MenuButton>

                  <MenuContent>
                    <SelectedMenuItem
                      name={replaceExistingFiles.COMBINE}
                      isSelected={!replaceExistingFiles}
                      onPress={this.onReplaceExistingFilesChange}
                    >
                      Combine With Existing Files
                    </SelectedMenuItem>

                    <SelectedMenuItem
                      name={replaceExistingFilesOptions.DELETE}
                      isSelected={replaceExistingFiles}
                      onPress={this.onReplaceExistingFilesChange}
                    >
                      Replace Existing Files
                    </SelectedMenuItem>
                  </MenuContent>
                </Menu>
            }
          </div>

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
                horizontalScroll={false}
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
                          isSaving={isSaving}
                          {...item}
                          allowAuthorChange={allowAuthorChange}
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
              'No book files were found in the selected folder'
          }
        </ModalBody>

        <ModalFooter className={styles.footer}>
          <div className={styles.leftButtons}>
            {
              !downloadId && showImportMode ?
                <SelectInput
                  className={styles.importMode}
                  name="importMode"
                  value={importMode}
                  values={importModeOptions}
                  onChange={this.onImportModeChange}
                /> :
                null
            }

            <SelectInput
              className={styles.bulkSelect}
              name="select"
              value={SELECT}
              values={bulkSelectOptions}
              isDisabled={!selectedIds.length}
              onChange={this.onSelectModalSelect}
            />
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
              isDisabled={!selectedIds.length || !!invalidRowsSelected.length || inconsistentBookReleases}
              onPress={this.onImportSelectedPress}
            >
              Import
            </Button>
          </div>
        </ModalFooter>

        <SelectAuthorModal
          isOpen={selectModalOpen === AUTHOR}
          ids={selectedIds}
          onModalClose={this.onSelectModalClose}
        />

        <SelectBookModal
          isOpen={selectModalOpen === BOOK}
          ids={selectedIds}
          authorId={selectedItem && selectedItem.author && selectedItem.author.id}
          onModalClose={this.onSelectModalClose}
        />

        <SelectEditionModal
          isOpen={selectModalOpen === EDITION}
          importIdsByBook={_.chain(items).filter((x) => x.album).groupBy((x) => x.book.id).mapValues((x) => x.map((y) => y.id)).value()}
          books={_.chain(items).filter((x) => x.book).keyBy((x) => x.book.id).mapValues((x) => ({ matchedEditionId: x.editionId, book: x.book })).values().value()}
          onModalClose={this.onSelectModalClose}
        />

        <SelectQualityModal
          isOpen={selectModalOpen === QUALITY}
          ids={selectedIds}
          qualityId={0}
          proper={false}
          real={false}
          onModalClose={this.onSelectModalClose}
        />

        <ConfirmImportModal
          isOpen={isConfirmImportModalOpen}
          books={booksImported}
          onModalClose={this.onConfirmImportModalClose}
          onConfirmImportPress={this.onConfirmImportPress}
        />

      </ModalContent>
    );
  }
}

InteractiveImportModalContent.propTypes = {
  downloadId: PropTypes.string,
  allowAuthorChange: PropTypes.bool.isRequired,
  showImportMode: PropTypes.bool.isRequired,
  showFilterExistingFiles: PropTypes.bool.isRequired,
  showReplaceExistingFiles: PropTypes.bool.isRequired,
  filterExistingFiles: PropTypes.bool.isRequired,
  replaceExistingFiles: PropTypes.bool.isRequired,
  importMode: PropTypes.string.isRequired,
  title: PropTypes.string,
  folder: PropTypes.string,
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  isSaving: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  sortKey: PropTypes.string,
  sortDirection: PropTypes.string,
  interactiveImportErrorMessage: PropTypes.string,
  onSortPress: PropTypes.func.isRequired,
  onFilterExistingFilesChange: PropTypes.func.isRequired,
  onReplaceExistingFilesChange: PropTypes.func.isRequired,
  onImportModeChange: PropTypes.func.isRequired,
  onImportSelectedPress: PropTypes.func.isRequired,
  saveInteractiveImportItem: PropTypes.func.isRequired,
  updateInteractiveImportItem: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

InteractiveImportModalContent.defaultProps = {
  allowAuthorChange: true,
  showFilterExistingFiles: false,
  showReplaceExistingFiles: false,
  showImportMode: true,
  importMode: 'move'
};

export default InteractiveImportModalContent;
