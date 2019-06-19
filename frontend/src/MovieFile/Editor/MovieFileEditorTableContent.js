import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import hasDifferentItems from 'Utilities/Object/hasDifferentItems';
import getSelectedIds from 'Utilities/Table/getSelectedIds';
import removeOldSelectedState from 'Utilities/Table/removeOldSelectedState';
import selectAll from 'Utilities/Table/selectAll';
import toggleSelected from 'Utilities/Table/toggleSelected';
import { kinds } from 'Helpers/Props';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import SpinnerButton from 'Components/Link/SpinnerButton';
import SelectInput from 'Components/Form/SelectInput';
import ModalFooter from 'Components/Modal/ModalFooter';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import MovieFileEditorRow from './MovieFileEditorRow';
import styles from './MovieFileEditorModalContent.css';

const columns = [
  {
    name: 'title',
    label: 'Title',
    isVisible: true
  },
  {
    name: 'mediainfo',
    label: 'Media Info',
    isVisible: true
  },
  {
    name: 'language',
    label: 'Language',
    isVisible: true
  },
  {
    name: 'quality',
    label: 'Quality',
    isVisible: true
  }
];

class MovieFileEditorTableContent extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      allSelected: false,
      allUnselected: false,
      lastToggled: null,
      selectedState: {},
      isConfirmDeleteModalOpen: false
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

  getSelectedIds = () => {
    const selectedIds = getSelectedIds(this.state.selectedState);

    return selectedIds.reduce((acc, id) => {
      const matchingItem = this.props.items.find((item) => item.id === id);

      if (matchingItem && !acc.includes(matchingItem.movieId)) {
        acc.push(matchingItem.movieId);
      }

      return acc;
    }, []);
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

  onDeletePress = () => {
    this.setState({ isConfirmDeleteModalOpen: true });
  }

  onConfirmDelete = () => {
    this.setState({ isConfirmDeleteModalOpen: false });
    this.props.onDeletePress(this.getSelectedIds());
  }

  onConfirmDeleteModalClose = () => {
    this.setState({ isConfirmDeleteModalOpen: false });
  }

  onLanguageChange = ({ value }) => {
    const selectedIds = this.getSelectedIds();
    if (!selectedIds.length) {
      return;
    }
    this.props.onLanguageChange(selectedIds, parseInt(value));
  }

  onQualityChange = ({ value }) => {
    const selectedIds = this.getSelectedIds();
    if (!selectedIds.length) {
      return;
    }
    this.props.onQualityChange(selectedIds, parseInt(value));
  }

  //
  // Render

  render() {
    const {
      isDeleting,
      items,
      languages,
      qualities
    } = this.props;

    const {
      allSelected,
      allUnselected,
      selectedState,
      isConfirmDeleteModalOpen
    } = this.state;

    const languageOptions = _.reduceRight(languages, (acc, language) => {
      acc.push({
        key: language.id,
        value: language.name
      });

      return acc;
    }, [{ key: 'selectLanguage', value: 'Select Language', disabled: true }]);

    const qualityOptions = _.reduceRight(qualities, (acc, quality) => {
      acc.push({
        key: quality.id,
        value: quality.name
      });

      return acc;
    }, [{ key: 'selectQuality', value: 'Select Quality', disabled: true }]);

    const hasSelectedFiles = this.getSelectedIds().length > 0;

    return (
      <div>
        {
          !items.length &&
            <div>
              No movie files to manage.
            </div>
        }

        {
          !!items.length &&
            <Table
              columns={columns}
              selectAll={true}
              allSelected={allSelected}
              allUnselected={allUnselected}
              onSelectAllChange={this.onSelectAllChange}
            >
              <TableBody>
                {
                  items.map((item) => {
                    return (
                      <MovieFileEditorRow
                        key={item.id}
                        isSelected={selectedState[item.id]}
                        {...item}
                        onSelectedChange={this.onSelectedChange}
                      />
                    );
                  })
                }
              </TableBody>
            </Table>
        }

        <ModalFooter>
          <div className={styles.actions}>
            <SpinnerButton
              kind={kinds.DANGER}
              isSpinning={isDeleting}
              isDisabled={!hasSelectedFiles}
              onPress={this.onDeletePress}
            >
              Delete
            </SpinnerButton>

            <div className={styles.selectInput}>
              <SelectInput
                name="language"
                value="selectLanguage"
                values={languageOptions}
                isDisabled={!hasSelectedFiles}
                onChange={this.onLanguageChange}
              />
            </div>

            <div className={styles.selectInput}>
              <SelectInput
                name="quality"
                value="selectQuality"
                values={qualityOptions}
                isDisabled={!hasSelectedFiles}
                onChange={this.onQualityChange}
              />
            </div>
          </div>
        </ModalFooter>

        <ConfirmModal
          isOpen={isConfirmDeleteModalOpen}
          kind={kinds.DANGER}
          title="Delete Selected Movie Files"
          message={'Are you sure you want to delete the selected movie files?'}
          confirmLabel="Delete"
          onConfirm={this.onConfirmDelete}
          onCancel={this.onConfirmDeleteModalClose}
        />
      </div>
    );
  }
}

MovieFileEditorTableContent.propTypes = {
  movieId: PropTypes.number,
  isDeleting: PropTypes.bool.isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  languages: PropTypes.arrayOf(PropTypes.object).isRequired,
  qualities: PropTypes.arrayOf(PropTypes.object).isRequired,
  onDeletePress: PropTypes.func.isRequired,
  onLanguageChange: PropTypes.func.isRequired,
  onQualityChange: PropTypes.func.isRequired
};

export default MovieFileEditorTableContent;
