import PropTypes from 'prop-types';
import React from 'react';
import Label from 'Components/Label';

function EpisodeLanguage(props) {
  const {
    className,
    language
  } = props;

  if (!language) {
    return null;
  }

  return (
    <Label className={className}>
      {language.name}
    </Label>
  );
}

EpisodeLanguage.propTypes = {
  className: PropTypes.string,
  language: PropTypes.object
};

export default EpisodeLanguage;
