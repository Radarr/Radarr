import PropTypes from 'prop-types';
import React from 'react';
import Link from 'Components/Link/Link';

function AuthorNameLink({ titleSlug, authorName }) {
  const link = `/author/${titleSlug}`;

  return (
    <Link to={link}>
      {authorName}
    </Link>
  );
}

AuthorNameLink.propTypes = {
  titleSlug: PropTypes.string.isRequired,
  authorName: PropTypes.string.isRequired
};

export default AuthorNameLink;
