import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import { icons } from 'Helpers/Props';
import ImportMovieTitle from './ImportMovieTitle';
import styles from './ImportMovieSearchResult.css';

class ImportMovieSearchResult extends Component {

  //
  // Listeners

  onPress = () => {
    this.props.onPress(this.props.tmdbId);
  };

  //
  // Render

  render() {
    const {
      tmdbId,
      title,
      year,
      studio,
      isExistingMovie
    } = this.props;

    return (
      <div className={styles.container}>
        <Link
          className={styles.movie}
          onPress={this.onPress}
        >
          <ImportMovieTitle
            title={title}
            year={year}
            network={studio}
            isExistingMovie={isExistingMovie}
          />
        </Link>

        <Link
          className={styles.tmdbLink}
          to={`https://www.themoviedb.org/movie/${tmdbId}`}
        >
          <Icon
            className={styles.tmdbLinkIcon}
            name={icons.EXTERNAL_LINK}
            size={16}
          />
        </Link>
      </div>
    );
  }
}

ImportMovieSearchResult.propTypes = {
  tmdbId: PropTypes.number.isRequired,
  title: PropTypes.string.isRequired,
  year: PropTypes.number.isRequired,
  studio: PropTypes.string,
  isExistingMovie: PropTypes.bool.isRequired,
  onPress: PropTypes.func.isRequired
};

export default ImportMovieSearchResult;
