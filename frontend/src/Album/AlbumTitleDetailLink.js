import PropTypes from 'prop-types';
import React from 'react';
import Link from 'Components/Link/Link';

function AlbumTitleDetailLink({ foreignAlbumId, title }) {
  const link = `/album/${foreignAlbumId}`;

  return (
    <Link to={link}>
      {title}
    </Link>
  );
}

AlbumTitleDetailLink.propTypes = {
  foreignAlbumId: PropTypes.string.isRequired,
  title: PropTypes.string.isRequired
};

export default AlbumTitleDetailLink;
