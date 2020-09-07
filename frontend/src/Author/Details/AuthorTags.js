import PropTypes from 'prop-types';
import React from 'react';
import Label from 'Components/Label';
import { kinds, sizes } from 'Helpers/Props';

function AuthorTags({ tags }) {
  return (
    <div>
      {
        tags.map((tag) => {
          return (
            <Label
              key={tag}
              kind={kinds.INFO}
              size={sizes.LARGE}
            >
              {tag}
            </Label>
          );
        })
      }
    </div>
  );
}

AuthorTags.propTypes = {
  tags: PropTypes.arrayOf(PropTypes.string).isRequired
};

export default AuthorTags;
