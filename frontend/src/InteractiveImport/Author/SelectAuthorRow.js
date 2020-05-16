import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Link from 'Components/Link/Link';
import styles from './SelectAuthorRow.css';

class SelectAuthorRow extends Component {

  //
  // Listeners

  onPress = () => {
    this.props.onAuthorSelect(this.props.id);
  }

  //
  // Render

  render() {
    return (
      <Link
        className={styles.author}
        component="div"
        onPress={this.onPress}
      >
        {this.props.authorName}
      </Link>
    );
  }
}

SelectAuthorRow.propTypes = {
  id: PropTypes.number.isRequired,
  authorName: PropTypes.string.isRequired,
  onAuthorSelect: PropTypes.func.isRequired
};

export default SelectAuthorRow;
