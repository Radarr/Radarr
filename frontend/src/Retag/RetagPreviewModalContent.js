import PropTypes from 'prop-types';
import React, { Component } from 'react';
import getSelectedIds from 'Utilities/Table/getSelectedIds';
import selectAll from 'Utilities/Table/selectAll';
import toggleSelected from 'Utilities/Table/toggleSelected';
import { kinds } from 'Helpers/Props';
import Alert from 'Components/Alert';
import Button from 'Components/Link/Button';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';
import CheckInput from 'Components/Form/CheckInput';
import RetagPreviewRow from './RetagPreviewRow';
import styles from './RetagPreviewModalContent.css';

function getValue(allSelected, allUnselected) {
  if (allSelected) {
    return true;
  } else if (allUnselected) {
    return false;
  }

  return null;
}

class RetagPreviewModalContent extends Component {

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

  onRetagPress = () => {
    this.props.onRetagPress(this.getSelectedIds());
  }

  //
  // Render

  render() {
    const {
      isFetching,
      isPopulated,
      error,
      items,
      path,
      onModalClose
    } = this.props;

    const {
      allSelected,
      allUnselected,
      selectedState
    } = this.state;

    const selectAllValue = getValue(allSelected, allUnselected);

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          Write Metadata Tags
        </ModalHeader>

        <ModalBody>
          {
            isFetching &&
              <LoadingIndicator />
          }

          {
            !isFetching && error &&
              <div>Error loading previews</div>
          }

          {
            !isFetching && ((isPopulated && !items.length)) &&
              <div>Success! My work is done, no files to retag.</div>
          }

          {
            !isFetching && isPopulated && !!items.length &&
              <div>
                <Alert>
                  <div>
                    All paths are relative to:
                    <span className={styles.path}>
                      {path}
                    </span>
                  </div>
                  <div>
                    MusicBrainz identifiers will also be added to the files; these are not shown below.
                  </div>
                </Alert>

                <div className={styles.previews}>
                  {
                    items.map((item) => {
                      return (
                        <RetagPreviewRow
                          key={item.trackFileId}
                          id={item.trackFileId}
                          path={item.relativePath}
                          changes={item.changes}
                          isSelected={selectedState[item.trackFileId]}
                          onSelectedChange={this.onSelectedChange}
                        />
                      );
                    })
                  }
                </div>
              </div>
          }
        </ModalBody>

        <ModalFooter>
          {
            isPopulated && !!items.length &&
              <CheckInput
                className={styles.selectAllInput}
                containerClassName={styles.selectAllInputContainer}
                name="selectAll"
                value={selectAllValue}
                onChange={this.onSelectAllChange}
              />
          }

          <Button
            onPress={onModalClose}
          >
            Cancel
          </Button>

          <Button
            kind={kinds.PRIMARY}
            onPress={this.onRetagPress}
          >
            Retag
          </Button>
        </ModalFooter>
      </ModalContent>
    );
  }
}

RetagPreviewModalContent.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  path: PropTypes.string.isRequired,
  onRetagPress: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default RetagPreviewModalContent;
