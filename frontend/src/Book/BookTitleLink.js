import PropTypes from 'prop-types';
import React from 'react';
import Link from 'Components/Link/Link';

function BookTitleLink({ titleSlug, title, disambiguation }) {
  const link = `/book/${titleSlug}`;

  return (
    <Link to={link}>
      {title}{disambiguation ? ` (${disambiguation})` : ''}
    </Link>
  );
}

BookTitleLink.propTypes = {
  titleSlug: PropTypes.string.isRequired,
  title: PropTypes.string.isRequired,
  disambiguation: PropTypes.string
};

export default BookTitleLink;
