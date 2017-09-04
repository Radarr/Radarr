import PropTypes from 'prop-types';
import React from 'react';
import Link from 'Components/Link/Link';

function ArtistNameLink({ nameSlug, artistName }) {
  const link = `/series/${nameSlug}`;

  return (
    <Link to={link}>
      {artistName}
    </Link>
  );
}

ArtistNameLink.propTypes = {
  nameSlug: PropTypes.string.isRequired,
  artistName: PropTypes.string.isRequired
};

export default ArtistNameLink;
