import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Link from 'Components/Link/Link';
import styles from './SelectAlbumRow.css';

class SelectAlbumRow extends Component {

  //
  // Listeners

  onPress = () => {
    this.props.onAlbumSelect(this.props.id);
  }

  //
  // Render

  render() {
    return (
      <Link
        className={styles.season}
        component="div"
        onPress={this.onPress}
      >
        {this.props.title} ({this.props.albumType})
      </Link>
    );
  }
}

SelectAlbumRow.propTypes = {
  id: PropTypes.number.isRequired,
  title: PropTypes.string.isRequired,
  albumType: PropTypes.string.isRequired,
  onAlbumSelect: PropTypes.func.isRequired
};

export default SelectAlbumRow;
