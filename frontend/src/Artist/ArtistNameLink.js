import PropTypes from 'prop-types';
import React from 'react';
import Link from 'Components/Link/Link';

function ArtistNameLink({ foreignArtistId, artistName }) {
  const link = `/artist/${foreignArtistId}`;

  return (
    <Link to={link}>
      {artistName}
    </Link>
  );
}

ArtistNameLink.propTypes = {
  foreignArtistId: PropTypes.string.isRequired,
  artistName: PropTypes.string.isRequired
};

export default ArtistNameLink;
