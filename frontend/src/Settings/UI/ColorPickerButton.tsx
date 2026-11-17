import React, { ChangeEvent, useCallback } from 'react';
import { InputChanged } from 'typings/inputs';
import styles from './ColorPickerButton.css';

interface ColorPickerButtonProps {
  name: string;
  value: string;
  // Set by FormInputGroup on buttons
  isLastButton?: boolean;
  onChange: (change: InputChanged<string>) => void;
}

const HEX_COLOR_REGEX = /^#[0-9a-f]{6}$/i;

// Native color picker shown alongside a text input holding the hex value
function ColorPickerButton({ name, value, onChange }: ColorPickerButtonProps) {
  const handleChange = useCallback(
    ({ target }: ChangeEvent<HTMLInputElement>) => {
      onChange({ name, value: target.value });
    },
    [name, onChange]
  );

  return (
    <input
      className={styles.colorPicker}
      type="color"
      value={HEX_COLOR_REGEX.test(value) ? value : '#000000'}
      onChange={handleChange}
    />
  );
}

export default ColorPickerButton;
