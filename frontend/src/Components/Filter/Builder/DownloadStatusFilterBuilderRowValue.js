import React from 'react';
import translate from 'Utilities/String/translate';
import FilterBuilderRowValue from './FilterBuilderRowValue';

const protocols = [
  {
    id: 'downloaded',
    get name() {
      return translate('DownloadedAndMonitored');
    }
  },
  {
    id: 'unmonitored',
    get name() {
      return translate('DownloadedButNotMonitored');
    }
  },
  {
    id: 'missingMonitored',
    get name() {
      return translate('MissingMonitoredAndConsideredAvailable');
    }
  },
  {
    id: 'missingUnmonitored',
    get name() {
      return translate('MissingNotMonitored');
    }
  },
  {
    id: 'queue',
    get name() {
      return translate('Queued');
    }
  },
  {
    id: 'continuing',
    get name() {
      return translate('Unreleased');
    }
  },
];

function DownloadStatusFilterBuilderRowValue(props) {
  return (
    <FilterBuilderRowValue
      tagList={protocols}
      {...props}
    />
  );
}

export default DownloadStatusFilterBuilderRowValue;
