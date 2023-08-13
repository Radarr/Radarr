import React from 'react';
import translate from 'Utilities/String/translate';
import FilterBuilderRowValue from './FilterBuilderRowValue';

const statusTagList = [
  { id: 'tba',
    get name() {
      return translate('Tba');
    } },
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
  },
  {
    id: 'deleted',
    get name() {
      return translate('Deleted');
    }
  }
];

function ReleaseStatusFilterBuilderRowValue(props) {
  return (
    <FilterBuilderRowValue
      tagList={statusTagList}
      {...props}
    />
  );
}

export default ReleaseStatusFilterBuilderRowValue;
