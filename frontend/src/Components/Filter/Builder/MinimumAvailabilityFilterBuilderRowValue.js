import React from 'react';
import translate from 'Utilities/String/translate';
import FilterBuilderRowValue from './FilterBuilderRowValue';

const protocols = [
  { id: 'announced', name: translate('Announced') },
  { id: 'inCinemas', name: translate('InCinemas') },
  { id: 'released', name: translate('Released') }
];

function MinimumAvailabilityFilterBuilderRowValue(props) {
  return (
    <FilterBuilderRowValue
      tagList={protocols}
      {...props}
    />
  );
}

export default MinimumAvailabilityFilterBuilderRowValue;
