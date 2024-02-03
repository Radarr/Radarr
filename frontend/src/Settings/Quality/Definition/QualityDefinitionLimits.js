import PropTypes from 'prop-types';
import React from 'react';
import formatBytes from 'Utilities/Number/formatBytes';
import translate from 'Utilities/String/translate';

function QualityDefinitionLimits(props) {
  const {
    bytes,
    message
  } = props;

  if (!bytes) {
    return <div>{message}</div>;
  }

  const sixty = formatBytes(bytes * 60);
  const ninety = formatBytes(bytes * 90);
  const hundredTwenty = formatBytes(bytes * 120);

  return (
    <div>
      <div>
        {translate('MinutesSixty', { sixty })}
      </div>
      <div>
        {translate('MinutesNinety', { ninety })}
      </div>
      <div>
        {translate('MinutesHundredTwenty', { hundredTwenty })}
      </div>
    </div>
  );
}

QualityDefinitionLimits.propTypes = {
  bytes: PropTypes.number,
  message: PropTypes.string.isRequired
};

export default QualityDefinitionLimits;
