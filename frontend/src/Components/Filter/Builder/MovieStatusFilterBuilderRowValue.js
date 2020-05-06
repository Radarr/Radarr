import React from 'react';
import FilterBuilderRowValue from './FilterBuilderRowValue';

const protocols = [
  { id: 'tba', name: 'TBA' },
  { id: 'announced', name: 'Announced' },
  { id: 'inCinemas', name: 'In Cinemas' },
  { id: 'released', name: 'Released' },
  { id: 'deleted', name: 'Deleted' }
];

function MovieStatusFilterBuilderRowValue(props) {
  return (
    <FilterBuilderRowValue
      tagList={protocols}
      {...props}
    />
  );
}

export default MovieStatusFilterBuilderRowValue;
