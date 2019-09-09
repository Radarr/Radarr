import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Link from 'Components/Link/Link';
import ImportArtistName from './ImportArtistName';
import styles from './ImportArtistSearchResult.css';

class ImportArtistSearchResult extends Component {

  //
  // Listeners

  onPress = () => {
    this.props.onPress(this.props.foreignArtistId);
  }

  //
  // Render

  render() {
    const {
      artistName,
      disambiguation,
      // year,
      isExistingArtist
    } = this.props;

    return (
      <Link
        className={styles.artist}
        onPress={this.onPress}
      >
        <ImportArtistName
          artistName={artistName}
          disambiguation={disambiguation}
          // year={year}
          isExistingArtist={isExistingArtist}
        />
      </Link>
    );
  }
}

ImportArtistSearchResult.propTypes = {
  foreignArtistId: PropTypes.string.isRequired,
  artistName: PropTypes.string.isRequired,
  disambiguation: PropTypes.string,
  // year: PropTypes.number.isRequired,
  isExistingArtist: PropTypes.bool.isRequired,
  onPress: PropTypes.func.isRequired
};

export default ImportArtistSearchResult;
