import classNames from 'classnames';
import PropTypes from 'prop-types';
import React from 'react';
import Button from 'Components/Link/Button';
import { kinds } from 'Helpers/Props';
import styles from './FormInputButton.css';

function FormInputButton(props) {
  const {
    className,
    ButtonComponent,
    isLastButton,
    ...otherProps
  } = props;

  return (
    <ButtonComponent
      className={classNames(
        className,
        !isLastButton && styles.middleButton
      )}
      kind={kinds.PRIMARY}
      {...otherProps}
    />
  );
}

FormInputButton.propTypes = {
  className: PropTypes.string.isRequired,
  ButtonComponent: PropTypes.elementType.isRequired,
  isLastButton: PropTypes.bool.isRequired
};

FormInputButton.defaultProps = {
  className: styles.button,
  ButtonComponent: Button,
  isLastButton: true
};

export default FormInputButton;
