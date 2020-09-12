import PropTypes from 'prop-types';
import React, { PureComponent } from 'react';
import Link from 'Components/Link/Link';

class MovieTitleLink extends PureComponent {

  render() {
    const {
      titleSlug,
      title
    } = this.props;

    const link = `/movie/${titleSlug}`;

    return (
      <Link
        to={link}
        title={title}
      >
        {title}
      </Link>
    );
  }
}

MovieTitleLink.propTypes = {
  titleSlug: PropTypes.string.isRequired,
  title: PropTypes.string.isRequired
};

export default MovieTitleLink;
