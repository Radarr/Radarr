import React from 'react';
import translate from 'Utilities/String/translate';
import FilterBuilderRowValue from './FilterBuilderRowValue';

const protocols = [
  { id: 'tba', name: 'TBA' },
  { id: 'announced', name: translate('Announced') },
  { id: 'inCinemas', name: translate('InCinemas') },
  { id: 'released', name: translate('Released') },
  { id: 'deleted', name: translate('Deleted') }
];

function ReleaseStatusFilterBuilderRowValue(props) {
  return (
    <FilterBuilderRowValue
      tagList={protocols}
      {...props}
    />
  );
}

export default ReleaseStatusFilterBuilderRowValue;
