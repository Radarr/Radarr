import React, { useCallback, useEffect } from 'react';
import { useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import createSortedSectionSelector from 'Store/Selectors/createSortedSectionSelector';
import CustomFormat from 'typings/CustomFormat';
import { EnhancedSelectInputChanged } from 'typings/inputs';
import sortByProp from 'Utilities/Array/sortByProp';
import EnhancedSelectInput, {
  EnhancedSelectInputProps,
  EnhancedSelectInputValue,
} from './EnhancedSelectInput';

const selectCustomFormats = createSelector(
  createSortedSectionSelector(
    'settings.customFormats',
    sortByProp<CustomFormat, 'name'>('name')
  ),
  (customFormats) => {
    return customFormats.items.map((customFormat) => {
      return {
        key: customFormat.id,
        value: customFormat.name,
      };
    }) as EnhancedSelectInputValue<number>[];
  }
);

export interface CustomFormatSelectInputProps
  extends Omit<
    EnhancedSelectInputProps<EnhancedSelectInputValue<number>, number>,
    'values'
  > {
  name: string;
}

function CustomFormatSelectInput({
  name,
  value,
  onChange,
  ...otherProps
}: CustomFormatSelectInputProps) {
  const values = useSelector(selectCustomFormats);

  const handleChange = useCallback(
    ({ value }: EnhancedSelectInputChanged<number>) => {
      onChange({ name, value });
    },
    [name, onChange]
  );

  useEffect(() => {
    if (!value || !values.some((option) => option.key === value)) {
      const firstValue = values[0];

      if (firstValue) {
        onChange({ name, value: firstValue.key });
      }
    }
  }, [name, value, values, onChange]);

  return (
    <EnhancedSelectInput
      {...otherProps}
      name={name}
      value={value}
      values={values}
      onChange={handleChange}
    />
  );
}

export default CustomFormatSelectInput;
