import PropTypes from 'prop-types';
import React from 'react';
import Link from 'Components/Link/Link';

function ArtistNameLink({ titleSlug, artistName }) {
  const link = `/author/${titleSlug}`;

  return (
    <Link to={link}>
      {artistName}
    </Link>
  );
}

ArtistNameLink.propTypes = {
  titleSlug: PropTypes.string.isRequired,
  artistName: PropTypes.string.isRequired
};

export default ArtistNameLink;
