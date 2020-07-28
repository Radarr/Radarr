import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Card from 'Components/Card';
import Label from 'Components/Label';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import { kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import EditNetImportModalConnector from './EditNetImportModalConnector';
import styles from './NetImport.css';

class NetImport extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isEditNetImportModalOpen: false,
      isDeleteNetImportModalOpen: false
    };
  }

  //
  // Listeners

  onEditNetImportPress = () => {
    this.setState({ isEditNetImportModalOpen: true });
  }

  onEditNetImportModalClose = () => {
    this.setState({ isEditNetImportModalOpen: false });
  }

  onDeleteNetImportPress = () => {
    this.setState({
      isEditNetImportModalOpen: false,
      isDeleteNetImportModalOpen: true
    });
  }

  onDeleteNetImportModalClose= () => {
    this.setState({ isDeleteNetImportModalOpen: false });
  }

  onConfirmDeleteNetImport = () => {
    this.props.onConfirmDeleteNetImport(this.props.id);
  }

  //
  // Render

  render() {
    const {
      id,
      name,
      enabled,
      enableAuto
    } = this.props;

    return (
      <Card
        className={styles.netImport}
        overlayContent={true}
        onPress={this.onEditNetImportPress}
      >
        <div className={styles.name}>
          {name}
        </div>

        <div className={styles.enabled}>

          {
            enabled &&
              <Label kind={kinds.SUCCESS}>
                Enabled
              </Label>
          }

          {
            enableAuto &&
              <Label kind={kinds.SUCCESS}>
                Auto
              </Label>
          }

          {
            !enabled && !enableAuto &&
              <Label
                kind={kinds.DISABLED}
                outline={true}
              >
                Disabled
              </Label>
          }
        </div>

        <EditNetImportModalConnector
          id={id}
          isOpen={this.state.isEditNetImportModalOpen}
          onModalClose={this.onEditNetImportModalClose}
          onDeleteNetImportPress={this.onDeleteNetImportPress}
        />

        <ConfirmModal
          isOpen={this.state.isDeleteNetImportModalOpen}
          kind={kinds.DANGER}
          title="Delete List"
          message={`Are you sure you want to delete the list '${name}'?`}
          confirmLabel={translate('Delete')}
          onConfirm={this.onConfirmDeleteNetImport}
          onCancel={this.onDeleteNetImportModalClose}
        />
      </Card>
    );
  }
}

NetImport.propTypes = {
  id: PropTypes.number.isRequired,
  name: PropTypes.string.isRequired,
  enabled: PropTypes.bool.isRequired,
  enableAuto: PropTypes.bool.isRequired,
  onConfirmDeleteNetImport: PropTypes.func.isRequired
};

export default NetImport;
