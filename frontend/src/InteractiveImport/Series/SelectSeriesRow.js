import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Link from 'Components/Link/Link';
import styles from './SelectSeriesRow.css';

class SelectSeriesRow extends Component {

  //
  // Listeners

  onPress = () => {
    this.props.onMovieSelect(this.props.id);
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

SelectSeriesRow.propTypes = {
  id: PropTypes.number.isRequired,
  title: PropTypes.string.isRequired,
  onMovieSelect: PropTypes.func.isRequired
};

export default SelectSeriesRow;
