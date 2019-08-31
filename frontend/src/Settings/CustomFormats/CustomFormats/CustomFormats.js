import PropTypes from 'prop-types';
import React, { Component } from 'react';
import sortByName from 'Utilities/Array/sortByName';
import { icons } from 'Helpers/Props';
import FieldSet from 'Components/FieldSet';
import Card from 'Components/Card';
import Icon from 'Components/Icon';
import PageSectionContent from 'Components/Page/PageSectionContent';
import CustomFormat from './CustomFormat';
import EditCustomFormatModalConnector from './EditCustomFormatModalConnector';
import styles from './CustomFormats.css';

class CustomFormats extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isCustomFormatModalOpen: false
    };
  }

  //
  // Listeners

  onCloneCustomFormatPress = (id) => {
    this.props.onCloneCustomFormatPress(id);
    this.setState({ isCustomFormatModalOpen: true });
  }

  onEditCustomFormatPress = () => {
    this.setState({ isCustomFormatModalOpen: true });
  }

  onModalClose = () => {
    this.setState({ isCustomFormatModalOpen: false });
  }

  //
  // Render

  render() {
    const {
      items,
      isDeleting,
      onConfirmDeleteCustomFormat,
      onCloneCustomFormatPress,
      ...otherProps
    } = this.props;

    return (
      <FieldSet legend="Custom Formats">
        <PageSectionContent
          errorMessage="Unable to load Custom Formats"
          {...otherProps}c={true}
        >
          <div className={styles.customFormats}>
            {
              items.sort(sortByName).map((item) => {
                return (
                  <CustomFormat
                    key={item.id}
                    {...item}
                    isDeleting={isDeleting}
                    onConfirmDeleteCustomFormat={onConfirmDeleteCustomFormat}
                    onCloneCustomFormatPress={this.onCloneCustomFormatPress}
                  />
                );
              })
            }

            <Card
              className={styles.addCustomFormat}
              onPress={this.onEditCustomFormatPress}
            >
              <div className={styles.center}>
                <Icon
                  name={icons.ADD}
                  size={45}
                />
              </div>
            </Card>
          </div>

          <EditCustomFormatModalConnector
            isOpen={this.state.isCustomFormatModalOpen}
            onModalClose={this.onModalClose}
          />

        </PageSectionContent>
      </FieldSet>
    );
  }
}

CustomFormats.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  isDeleting: PropTypes.bool.isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  onConfirmDeleteCustomFormat: PropTypes.func.isRequired,
  onCloneCustomFormatPress: PropTypes.func.isRequired
};

export default CustomFormats;
