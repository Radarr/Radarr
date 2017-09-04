import PropTypes from 'prop-types';
import React from 'react';
import styles from './Form.css';

function Form({ children, validationErrors, validationWarnings, ...otherProps }) {
  return (
    <div>
      <div>
        {
          validationErrors.map((error, index) => {
            return (
              <div
                key={index}
                className={styles.error}
              >
                {error.errorMessage}
              </div>
            );
          })
        }

        {
          validationWarnings.map((warning, index) => {
            return (
              <div
                key={index}
                className={styles.error}
              >
                {warning.errorMessage}
              </div>
            );
          })
        }
      </div>

      {children}
    </div>
  );
}

Form.propTypes = {
  children: PropTypes.node.isRequired,
  validationErrors: PropTypes.arrayOf(PropTypes.object).isRequired,
  validationWarnings: PropTypes.arrayOf(PropTypes.object).isRequired
};

Form.defaultProps = {
  validationErrors: [],
  validationWarnings: []
};

export default Form;
