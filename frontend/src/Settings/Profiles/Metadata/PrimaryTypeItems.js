import PropTypes from 'prop-types';
import React, { Component } from 'react';
import FormGroup from 'Components/Form/FormGroup';
import FormLabel from 'Components/Form/FormLabel';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import PrimaryTypeItem from './PrimaryTypeItem';
import styles from './TypeItems.css';

class PrimaryTypeItems extends Component {

  //
  // Render

  render() {
    const {
      metadataProfileItems,
      errors,
      warnings,
      ...otherProps
    } = this.props;

    return (
      <FormGroup>
        <FormLabel>Primary Types</FormLabel>
        <div>

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

          <div className={styles.albumTypes}>
            {
              metadataProfileItems.map(({ allowed, albumType }, index) => {
                return (
                  <PrimaryTypeItem
                    key={albumType.id}
                    albumTypeId={albumType.id}
                    name={albumType.name}
                    allowed={allowed}
                    sortIndex={index}
                    {...otherProps}
                  />
                );
              }).reverse()
            }
          </div>
        </div>
      </FormGroup>
    );
  }
}

PrimaryTypeItems.propTypes = {
  metadataProfileItems: PropTypes.arrayOf(PropTypes.object).isRequired,
  errors: PropTypes.arrayOf(PropTypes.object),
  warnings: PropTypes.arrayOf(PropTypes.object),
  formLabel: PropTypes.string
};

PrimaryTypeItems.defaultProps = {
  errors: [],
  warnings: []
};

export default PrimaryTypeItems;
