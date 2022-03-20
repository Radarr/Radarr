import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Link from 'Components/Link/Link';
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
      title,
      year,
      studio,
      isExistingMovie
    } = this.props;

    return (
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
