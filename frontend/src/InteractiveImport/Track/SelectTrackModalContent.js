import PropTypes from 'prop-types';
import React, { Component } from 'react';
import _ from 'lodash';
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
import ExpandingFileDetails from 'TrackFile/ExpandingFileDetails';

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
  },
  {
    name: 'trackStatus',
    label: 'Status',
    isVisible: true
  }
];

const selectAllBlankColumn = [
  {
    name: 'dummy',
    label: ' ',
    isVisible: true
  }
];

class SelectTrackModalContent extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    const selectedTracks = _.filter(props.selectedTracksByItem, ['id', props.id])[0].tracks;
    const init = _.zipObject(selectedTracks, _.times(selectedTracks.length, _.constant(true)));

    this.state = {
      allSelected: false,
      allUnselected: false,
      lastToggled: null,
      selectedState: init
    };

    props.onSortPress( props.sortKey, props.sortDirection );
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
      id,
      audioTags,
      rejections,
      isFetching,
      isPopulated,
      error,
      items,
      sortKey,
      sortDirection,
      onSortPress,
      onModalClose,
      selectedTracksByItem,
      filename
    } = this.props;

    const {
      allSelected,
      allUnselected,
      selectedState
    } = this.state;

    const errorMessage = getErrorMessage(error, 'Unable to load tracks');

    // all tracks selected for other items
    const otherSelected = _.map(_.filter(selectedTracksByItem, (item) => {
      return item.id !== id;
    }), (x) => {
      return x.tracks;
    }).flat();
    // tracks selected for the current file
    const currentSelected = _.keys(_.pickBy(selectedState, _.identity)).map(Number);
    // only enable selectAll if no other files have any tracks selected.
    const selectAllEnabled = otherSelected.length === 0;

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          Manual Import - Select Track(s):
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

          <ExpandingFileDetails
            audioTags={audioTags}
            filename={filename}
            rejections={rejections}
            isExpanded={false}
          />

          {
            isPopulated && !!items.length &&
              <Table
                columns={selectAllEnabled ? columns : selectAllBlankColumn.concat(columns)}
                selectAll={selectAllEnabled}
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
                          hasFile={item.hasFile}
                          importSelected={otherSelected.concat(currentSelected).includes(item.id)}
                          isDisabled={otherSelected.includes(item.id)}
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
  id: PropTypes.number.isRequired,
  rejections: PropTypes.arrayOf(PropTypes.object).isRequired,
  audioTags: PropTypes.object.isRequired,
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  sortKey: PropTypes.string,
  sortDirection: PropTypes.string,
  onSortPress: PropTypes.func.isRequired,
  onTracksSelect: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired,
  selectedTracksByItem: PropTypes.arrayOf(PropTypes.object).isRequired,
  filename: PropTypes.string.isRequired
};

export default SelectTrackModalContent;
