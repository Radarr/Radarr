import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Link from 'Components/Link/Link';
import styles from './SelectMovieRow.css';

class SelectMovieRow extends Component {

  //
  // Listeners

  onPress = () => {
    this.props.onMovieSelect(this.props.id);
  };

  //
  // Render

  render() {
    return (
      <Link
        className={styles.movie}
        component="div"
        onPress={this.onPress}
      >
        {this.props.title} ({this.props.year})
      </Link>
    );
  }
}

SelectMovieRow.propTypes = {
  id: PropTypes.number.isRequired,
  title: PropTypes.string.isRequired,
  year: PropTypes.number.isRequired,
  onMovieSelect: PropTypes.func.isRequired
};

export default SelectMovieRow;
