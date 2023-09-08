import React, { useCallback } from 'react';
import { useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import EnhancedSelectInput from './EnhancedSelectInput';

interface IndexerFlagsSelectInputProps {
  name: string;
  indexerFlags: number;
  onChange(payload: object): void;
}

const selectIndexerFlagsValues = (selectedFlags: number) =>
  createSelector(
    (state: AppState) => state.settings.indexerFlags,
    (indexerFlags) => {
      const value = indexerFlags.items
        .filter(
          // eslint-disable-next-line no-bitwise
          (item) => (selectedFlags & item.id) === item.id
        )
        .map(({ id }) => id);

      const values = indexerFlags.items.map(({ id, name }) => ({
        key: id,
        value: name,
      }));

      return {
        value,
        values,
      };
    }
  );

function IndexerFlagsSelectInput(props: IndexerFlagsSelectInputProps) {
  const { indexerFlags, onChange } = props;

  const { value, values } = useSelector(selectIndexerFlagsValues(indexerFlags));

  const onChangeWrapper = useCallback(
    ({ name, value }: { name: string; value: number[] }) => {
      const indexerFlags = value.reduce((acc, flagId) => acc + flagId, 0);

      onChange({ name, value: indexerFlags });
    },
    [onChange]
  );

  return (
    <EnhancedSelectInput
      {...props}
      value={value}
      values={values}
      onChange={onChangeWrapper}
    />
  );
}

export default IndexerFlagsSelectInput;
