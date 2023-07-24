import React from 'react';
import translate from 'Utilities/String/translate';
import FilterBuilderRowValue from './FilterBuilderRowValue';

const protocols = [
  {
    id: 'announced',
    get name() {
      return translate('Announced');
    }
  },
  {
    id: 'inCinemas',
    get name() {
      return translate('InCinemas');
    }
  },
  {
    id: 'released',
    get name() {
      return translate('Released');
    }
  }
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
