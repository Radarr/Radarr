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

  const twenty = formatBytes(bytes * 20 * 60);
  const fourtyFive = formatBytes(bytes * 45 * 60);
  const sixty = formatBytes(bytes * 60 * 60);

  return (
    <div>
      <div>20 Minutes: {twenty}</div>
      <div>45 Minutes: {fourtyFive}</div>
      <div>60 Minutes: {sixty}</div>
    </div>
  );
}

QualityDefinitionLimits.propTypes = {
  bytes: PropTypes.number,
  message: PropTypes.string.isRequired
};

export default QualityDefinitionLimits;
