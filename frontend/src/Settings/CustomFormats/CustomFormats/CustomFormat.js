import PropTypes from 'prop-types';
import React, { Component } from 'react';
import split from 'Utilities/String/split';
import { icons, kinds } from 'Helpers/Props';
import Card from 'Components/Card';
import Label from 'Components/Label';
import IconButton from 'Components/Link/IconButton';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import EditCustomFormatModalConnector from './EditCustomFormatModalConnector';
import styles from './CustomFormat.css';

class CustomFormat extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isEditCustomFormatModalOpen: false,
      isDeleteCustomFormatModalOpen: false
    };
  }

  //
  // Listeners

  onEditCustomFormatPress = () => {
    this.setState({ isEditCustomFormatModalOpen: true });
  }

  onEditCustomFormatModalClose = () => {
    this.setState({ isEditCustomFormatModalOpen: false });
  }

  onDeleteCustomFormatPress = () => {
    this.setState({
      isEditCustomFormatModalOpen: false,
      isDeleteCustomFormatModalOpen: true
    });
  }

  onDeleteCustomFormatModalClose = () => {
    this.setState({ isDeleteCustomFormatModalOpen: false });
  }

  onConfirmDeleteCustomFormat = () => {
    this.props.onConfirmDeleteCustomFormat(this.props.id);
  }

  onCloneCustomFormatPress = () => {
    const {
      id,
      onCloneCustomFormatPress
    } = this.props;

    onCloneCustomFormatPress(id);
  }

  //
  // Render

  render() {
    const {
      id,
      name,
      formatTags,
      isDeleting
    } = this.props;

    return (
      <Card
        className={styles.customFormat}
        overlayContent={true}
        onPress={this.onEditCustomFormatPress}
      >
        <div className={styles.nameContainer}>
          <div className={styles.name}>
            {name}
          </div>

          <IconButton
            className={styles.cloneButton}
            title="Clone Profile"
            name={icons.CLONE}
            onPress={this.onCloneCustomFormatPress}
          />
        </div>

        <div>
          {
            split(formatTags).map((item) => {
              if (!item) {
                return null;
              }

              return (
                <Label
                  key={item}
                  kind={kinds.DEFAULT}
                >
                  {item}
                </Label>
              );
            })
          }
        </div>

        <EditCustomFormatModalConnector
          id={id}
          isOpen={this.state.isEditCustomFormatModalOpen}
          onModalClose={this.onEditCustomFormatModalClose}
          onDeleteCustomFormatPress={this.onDeleteCustomFormatPress}
        />

        <ConfirmModal
          isOpen={this.state.isDeleteCustomFormatModalOpen}
          kind={kinds.DANGER}
          title="Delete Custom Format"
          message={`Are you sure you want to delete the custom format '${name}'?`}
          confirmLabel="Delete"
          isSpinning={isDeleting}
          onConfirm={this.onConfirmDeleteCustomFormat}
          onCancel={this.onDeleteCustomFormatModalClose}
        />
      </Card>
    );
  }
}

CustomFormat.propTypes = {
  id: PropTypes.number.isRequired,
  name: PropTypes.string.isRequired,
  formatTags: PropTypes.string.isRequired,
  isDeleting: PropTypes.bool.isRequired,
  onConfirmDeleteCustomFormat: PropTypes.func.isRequired,
  onCloneCustomFormatPress: PropTypes.func.isRequired
};

export default CustomFormat;
