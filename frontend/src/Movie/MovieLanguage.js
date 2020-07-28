import PropTypes from 'prop-types';
import React from 'react';
import Label from 'Components/Label';
import Popover from 'Components/Tooltip/Popover';
import { kinds, tooltipPositions } from 'Helpers/Props';

function MovieLanguage(props) {
  const {
    className,
    languages,
    isCutoffNotMet
  } = props;

  if (!languages) {
    return null;
  }

  if (languages.length === 1) {
    return (
      <Label
        className={className}
        kind={isCutoffNotMet ? kinds.INVERSE : kinds.DEFAULT}
      >
        {languages[0].name}
      </Label>
    );
  }

  return (
    <Popover
      className={className}
      anchor={
        <Label
          className={className}
          kind={isCutoffNotMet ? kinds.INVERSE : kinds.DEFAULT}
        >
          Multi-Language
        </Label>
      }
      title="Languages"
      body={
        <ul>
          {
            languages.map((language) => {
              return (
                <li key={language.id}>
                  {language.name}
                </li>
              );
            })
          }
        </ul>
      }
      position={tooltipPositions.LEFT}
    />
  );
}

MovieLanguage.propTypes = {
  className: PropTypes.string,
  languages: PropTypes.arrayOf(PropTypes.object),
  isCutoffNotMet: PropTypes.bool
};

MovieLanguage.defaultProps = {
  isCutoffNotMet: true
};

export default MovieLanguage;
