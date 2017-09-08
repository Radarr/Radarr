import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Link from 'Components/Link/Link';
import styles from './SelectArtistRow.css';

class SelectArtistRow extends Component {

  //
  // Listeners

  onPress = () => {
    this.props.onSeriesSelect(this.props.id);
  }

  //
  // Render

  render() {
    return (
      <Link
        className={styles.series}
        component="div"
        onPress={this.onPress}
      >
        {this.props.title}
      </Link>
    );
  }
}

SelectArtistRow.propTypes = {
  id: PropTypes.number.isRequired,
  title: PropTypes.string.isRequired,
  onSeriesSelect: PropTypes.func.isRequired
};

export default SelectArtistRow;
