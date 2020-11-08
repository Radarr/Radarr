import PropTypes from 'prop-types';
import React from 'react';
import Label from 'Components/Label';
import { kinds } from 'Helpers/Props';

function MovieFormats({ formats }) {
  return (
    <div>
      {
        formats.map((format) => {
          return (
            <Label
              key={format.id}
              kind={kinds.INFO}
            >
              {format.name}
            </Label>
          );
        })
      }
    </div>
  );
}

MovieFormats.propTypes = {
  formats: PropTypes.arrayOf(PropTypes.object).isRequired
};

MovieFormats.defaultProps = {
  formats: []
};

export default MovieFormats;
