import PropTypes from 'prop-types';
import React from 'react';
import Label from 'Components/Label';
import { kinds, sizes } from 'Helpers/Props';

function PageSidebarStatus({ queue, errors, warnings }) {
  if (!queue && !warnings && !errors) {
    return null;
  }

  return (
    <>
      {
        queue > 0 &&
          <Label
            kind={kinds.QUEUE}
            size={sizes.MEDIUM}
          >
            {queue}
          </Label>
      }

      {
        warnings > 0 &&
          <Label
            kind={kinds.WARNING}
            size={sizes.MEDIUM}
          >
            {warnings}
          </Label>
      }

      {
        errors > 0 &&
          <Label
            kind={kinds.DANGER}
            size={sizes.MEDIUM}
          >
            {errors}
          </Label>
      }
    </>
  );
}

PageSidebarStatus.propTypes = {
  queue: PropTypes.number,
  errors: PropTypes.number,
  warnings: PropTypes.number
};

export default PageSidebarStatus;
