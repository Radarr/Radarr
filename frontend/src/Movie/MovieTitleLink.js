import PropTypes from 'prop-types';
import React, { PureComponent } from 'react';
import Link from 'Components/Link/Link';

class MovieTitleLink extends PureComponent {

  render() {
    const {
      titleSlug,
      title,
      year
    } = this.props;

    const link = `/movie/${titleSlug}`;

    return (
      <Link
        to={link}
        title={title}
      >
        {title}{year > 0 ? ` (${year})` : ''}
      </Link>
    );
  }
}

MovieTitleLink.propTypes = {
  titleSlug: PropTypes.string.isRequired,
  title: PropTypes.string.isRequired,
  year: PropTypes.number
};

export default MovieTitleLink;
