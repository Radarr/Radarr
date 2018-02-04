import PropTypes from 'prop-types';
import React, { Component } from 'react';
import getErrorMessage from 'Utilities/Object/getErrorMessage';
import getSelectedIds from 'Utilities/Table/getSelectedIds';
import selectAll from 'Utilities/Table/selectAll';
import toggleSelected from 'Utilities/Table/toggleSelected';
import { kinds } from 'Helpers/Props';
import Button from 'Components/Link/Button';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import SelectTrackRow from './SelectTrackRow';

const columns = [
  {
    name: 'mediumNumber',
    label: 'Medium',
    isSortable: true,
    isVisible: true
  },
  {
    name: 'trackNumber',
    label: '#',
    isSortable: true,
    isVisible: true
  },
  {
    name: 'title',
    label: 'Title',
    isVisible: true
  }
];

class SelectTrackModalContent extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      allSelected: false,
      allUnselected: false,
      lastToggled: null,
      selectedState: {}
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

  onTracksSelect = () => {
    this.props.onTracksSelect(this.getSelectedIds());
  }

  //
  // Render

  render() {
    const {
      isFetching,
      isPopulated,
      error,
      items,
      sortKey,
      sortDirection,
      onSortPress,
      onModalClose
    } = this.props;

    const {
      allSelected,
      allUnselected,
      selectedState
    } = this.state;

    const errorMessage = getErrorMessage(error, 'Unable to load tracks');

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          Manual Import - Select Track(s)
        </ModalHeader>

        <ModalBody>
          {
            isFetching &&
              <LoadingIndicator />
          }

          {
            error &&
              <div>{errorMessage}</div>
          }

          {
            isPopulated && !!items.length &&
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
                        <SelectTrackRow
                          key={item.id}
                          id={item.id}
                          mediumNumber={item.mediumNumber}
                          trackNumber={item.absoluteTrackNumber}
                          title={item.title}
                          isSelected={selectedState[item.id]}
                          onSelectedChange={this.onSelectedChange}
                        />
                      );
                    })
                  }
                </TableBody>
              </Table>
          }

          {
            isPopulated && !items.length &&
              'No tracks were found for the selected album'
          }
        </ModalBody>

        <ModalFooter>
          <Button onPress={onModalClose}>
            Cancel
          </Button>

          <Button
            kind={kinds.SUCCESS}
            onPress={this.onTracksSelect}
          >
            Select Tracks
          </Button>
        </ModalFooter>
      </ModalContent>
    );
  }
}

SelectTrackModalContent.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  sortKey: PropTypes.string,
  sortDirection: PropTypes.string,
  onSortPress: PropTypes.func.isRequired,
  onTracksSelect: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default SelectTrackModalContent;
