import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Card from 'Components/Card';
import Label from 'Components/Label';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import { kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import EditNotificationModalConnector from './EditNotificationModalConnector';
import styles from './Notification.css';

class Notification extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isEditNotificationModalOpen: false,
      isDeleteNotificationModalOpen: false
    };
  }

  //
  // Listeners

  onEditNotificationPress = () => {
    this.setState({ isEditNotificationModalOpen: true });
  }

  onEditNotificationModalClose = () => {
    this.setState({ isEditNotificationModalOpen: false });
  }

  onDeleteNotificationPress = () => {
    this.setState({
      isEditNotificationModalOpen: false,
      isDeleteNotificationModalOpen: true
    });
  }

  onDeleteNotificationModalClose= () => {
    this.setState({ isDeleteNotificationModalOpen: false });
  }

  onConfirmDeleteNotification = () => {
    this.props.onConfirmDeleteNotification(this.props.id);
  }

  //
  // Render

  render() {
    const {
      id,
      name,
      onGrab,
      onDownload,
      onUpgrade,
      onRename,
      onDelete,
      onHealthIssue,
      supportsOnGrab,
      supportsOnDownload,
      supportsOnUpgrade,
      supportsOnRename,
      supportsOnDelete,
      supportsOnHealthIssue
    } = this.props;

    return (
      <Card
        className={styles.notification}
        overlayContent={true}
        onPress={this.onEditNotificationPress}
      >
        <div className={styles.name}>
          {name}
        </div>

        {
          supportsOnGrab && onGrab &&
            <Label kind={kinds.SUCCESS}>
              {translate('OnGrab')}
            </Label>
        }

        {
          supportsOnDelete && onDelete &&
            <Label kind={kinds.SUCCESS}>
              {translate('OnDelete')}
            </Label>
        }

        {
          supportsOnDownload && onDownload &&
            <Label kind={kinds.SUCCESS}>
              {translate('OnImport')}
            </Label>
        }

        {
          supportsOnUpgrade && onDownload && onUpgrade &&
            <Label kind={kinds.SUCCESS}>
              {translate('OnUpgrade')}
            </Label>
        }

        {
          supportsOnRename && onRename &&
            <Label kind={kinds.SUCCESS}>
              {translate('OnRename')}
            </Label>
        }

        {
          supportsOnHealthIssue && onHealthIssue &&
            <Label kind={kinds.SUCCESS}>
              {translate('OnHealthIssue')}
            </Label>
        }

        {
          !onGrab && !onDownload && !onRename && !onHealthIssue && !onDelete &&
            <Label
              kind={kinds.DISABLED}
              outline={true}
            >
              {translate('Disabled')}
            </Label>
        }

        <EditNotificationModalConnector
          id={id}
          isOpen={this.state.isEditNotificationModalOpen}
          onModalClose={this.onEditNotificationModalClose}
          onDeleteNotificationPress={this.onDeleteNotificationPress}
        />

        <ConfirmModal
          isOpen={this.state.isDeleteNotificationModalOpen}
          kind={kinds.DANGER}
          title={translate('DeleteNotification')}
          message={translate('DeleteNotificationMessageText', [name])}
          confirmLabel={translate('Delete')}
          onConfirm={this.onConfirmDeleteNotification}
          onCancel={this.onDeleteNotificationModalClose}
        />
      </Card>
    );
  }
}

Notification.propTypes = {
  id: PropTypes.number.isRequired,
  name: PropTypes.string.isRequired,
  onGrab: PropTypes.bool.isRequired,
  onDownload: PropTypes.bool.isRequired,
  onUpgrade: PropTypes.bool.isRequired,
  onRename: PropTypes.bool.isRequired,
  onDelete: PropTypes.bool.isRequired,
  onHealthIssue: PropTypes.bool.isRequired,
  supportsOnGrab: PropTypes.bool.isRequired,
  supportsOnDownload: PropTypes.bool.isRequired,
  supportsOnDelete: PropTypes.bool.isRequired,
  supportsOnUpgrade: PropTypes.bool.isRequired,
  supportsOnRename: PropTypes.bool.isRequired,
  supportsOnHealthIssue: PropTypes.bool.isRequired,
  onConfirmDeleteNotification: PropTypes.func.isRequired
};

export default Notification;
