import React from 'react';
import FilterBuilderRowValue from './FilterBuilderRowValue';

const protocols = [
  { id: 'announced', name: 'Announced' },
  { id: 'inCinemas', name: 'In Cinemas' },
  { id: 'released', name: 'Released' }
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
