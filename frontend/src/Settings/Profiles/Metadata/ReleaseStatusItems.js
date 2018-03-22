import PropTypes from 'prop-types';
import React, { Component } from 'react';
import FormGroup from 'Components/Form/FormGroup';
import FormLabel from 'Components/Form/FormLabel';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import ReleaseStatusItem from './ReleaseStatusItem';
import styles from './TypeItems.css';

class ReleaseStatusItems extends Component {

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
        <FormLabel>Release Statuses</FormLabel>
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
              metadataProfileItems.map(({ allowed, releaseStatus }, index) => {
                return (
                  <ReleaseStatusItem
                    key={releaseStatus.id}
                    albumTypeId={releaseStatus.id}
                    name={releaseStatus.name}
                    allowed={allowed}
                    sortIndex={index}
                    {...otherProps}
                  />
                );
              })
            }
          </div>
        </div>
      </FormGroup>
    );
  }
}

ReleaseStatusItems.propTypes = {
  metadataProfileItems: PropTypes.arrayOf(PropTypes.object).isRequired,
  errors: PropTypes.arrayOf(PropTypes.object),
  warnings: PropTypes.arrayOf(PropTypes.object),
  formLabel: PropTypes.string
};

ReleaseStatusItems.defaultProps = {
  errors: [],
  warnings: []
};

export default ReleaseStatusItems;
