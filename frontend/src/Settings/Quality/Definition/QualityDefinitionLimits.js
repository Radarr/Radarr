import PropTypes from 'prop-types';
import React from 'react';
import formatBytes from 'Utilities/Number/formatBytes';

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
      <div>60 Minutes: {sixty}</div>
      <div>90 Minutes: {ninety}</div>
      <div>120 Minutes: {hundredTwenty}</div>
    </div>
  );
}

QualityDefinitionLimits.propTypes = {
  bytes: PropTypes.number,
  message: PropTypes.string.isRequired
};

export default QualityDefinitionLimits;
