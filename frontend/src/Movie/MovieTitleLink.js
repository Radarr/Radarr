import PropTypes from 'prop-types';
import React from 'react';
import Link from 'Components/Link/Link';

function MovieTitleLink({ titleSlug, title }) {
  const link = `/movie/${titleSlug}`;

  return (
    <Link to={link}>
      {title}
    </Link>
  );
}

MovieTitleLink.propTypes = {
  titleSlug: PropTypes.string.isRequired,
  title: PropTypes.string.isRequired
};

export default MovieTitleLink;
