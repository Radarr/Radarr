import PropTypes from 'prop-types';
import React, { Component } from 'react';
import FormGroup from 'Components/Form/FormGroup';
import FormLabel from 'Components/Form/FormLabel';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import QualityProfileFormatItemDragSource from './QualityProfileFormatItemDragSource';
import QualityProfileFormatItemDragPreview from './QualityProfileFormatItemDragPreview';
import styles from './QualityProfileFormatItems.css';

class QualityProfileFormatItems extends Component {

  //
  // Render

  render() {
    const {
      dragIndex,
      dropIndex,
      profileFormatItems,
      errors,
      warnings,
      ...otherProps
    } = this.props;

    const isDragging = dropIndex !== null;
    const isDraggingUp = isDragging && dropIndex > dragIndex;
    const isDraggingDown = isDragging && dropIndex < dragIndex;

    return (
      <FormGroup>
        <FormLabel>Custom Formats</FormLabel>
        <div>
          <FormInputHelpText
            text="Custom Formats higher in the list are more preferred. Only checked custom formats are wanted"
          />

          {
            errors.map((error, index) => {
              return (
                <FormInputHelpText
                  key={index}
                  text={error.message}
                  isError={true}
                  isCheckInput={false}
                />
              );
            })
          }

          {
            warnings.map((warning, index) => {
              return (
                <FormInputHelpText
                  key={index}
                  text={warning.message}
                  isWarning={true}
                  isCheckInput={false}
                />
              );
            })
          }

          <div className={styles.formats}>
            {
              profileFormatItems.map(({ allowed, format }, index) => {
                return (
                  <QualityProfileFormatItemDragSource
                    key={format.id}
                    formatId={format.id}
                    name={format.name}
                    allowed={allowed}
                    sortIndex={index}
                    isDragging={isDragging}
                    isDraggingUp={isDraggingUp}
                    isDraggingDown={isDraggingDown}
                    {...otherProps}
                  />
                );
              }).reverse()
            }

            <QualityProfileFormatItemDragPreview />
          </div>
        </div>
      </FormGroup>
    );
  }
}

QualityProfileFormatItems.propTypes = {
  dragIndex: PropTypes.number,
  dropIndex: PropTypes.number,
  profileFormatItems: PropTypes.arrayOf(PropTypes.object).isRequired,
  errors: PropTypes.arrayOf(PropTypes.object),
  warnings: PropTypes.arrayOf(PropTypes.object)
};

QualityProfileFormatItems.defaultProps = {
  errors: [],
  warnings: []
};

export default QualityProfileFormatItems;
