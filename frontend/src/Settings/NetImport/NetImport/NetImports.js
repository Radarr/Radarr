import PropTypes from 'prop-types';
import React, { Component } from 'react';
import sortByName from 'Utilities/Array/sortByName';
import { icons } from 'Helpers/Props';
import FieldSet from 'Components/FieldSet';
import Card from 'Components/Card';
import Icon from 'Components/Icon';
import PageSectionContent from 'Components/Page/PageSectionContent';
import NetImport from './NetImport';
import AddNetImportModal from './AddNetImportModal';
import EditNetImportModalConnector from './EditNetImportModalConnector';
import styles from './NetImports.css';

class NetImports extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isAddNetImportModalOpen: false,
      isEditNetImportModalOpen: false
    };
  }

  //
  // Listeners

  onAddNetImportPress = () => {
    this.setState({ isAddNetImportModalOpen: true });
  }

  onAddNetImportModalClose = ({ netImportSelected = false } = {}) => {
    this.setState({
      isAddNetImportModalOpen: false,
      isEditNetImportModalOpen: netImportSelected
    });
  }

  onEditNetImportModalClose = () => {
    this.setState({ isEditNetImportModalOpen: false });
  }

  //
  // Render

  render() {
    const {
      items,
      onConfirmDeleteNetImport,
      ...otherProps
    } = this.props;

    const {
      isAddNetImportModalOpen,
      isEditNetImportModalOpen
    } = this.state;

    return (
      <FieldSet legend="Lists">
        <PageSectionContent
          errorMessage="Unable to load Lists"
          {...otherProps}
        >
          <div className={styles.netImports}>
            {
              items.sort(sortByName).map((item) => {
                return (
                  <NetImport
                    key={item.id}
                    {...item}
                    onConfirmDeleteNetImport={onConfirmDeleteNetImport}
                  />
                );
              })
            }

            <Card
              className={styles.addNetImport}
              onPress={this.onAddNetImportPress}
            >
              <div className={styles.center}>
                <Icon
                  name={icons.ADD}
                  size={45}
                />
              </div>
            </Card>
          </div>

          <AddNetImportModal
            isOpen={isAddNetImportModalOpen}
            onModalClose={this.onAddNetImportModalClose}
          />

          <EditNetImportModalConnector
            isOpen={isEditNetImportModalOpen}
            onModalClose={this.onEditNetImportModalClose}
          />
        </PageSectionContent>
      </FieldSet>
    );
  }
}

NetImports.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  onConfirmDeleteNetImport: PropTypes.func.isRequired
};

export default NetImports;
