import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Label from 'Components/Label';
import VirtualTableRowCell from 'Components/Table/Cells/VirtualTableRowCell';
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
      <>
        <VirtualTableRowCell className={styles.title}>
          {this.props.title}
        </VirtualTableRowCell>

        <VirtualTableRowCell className={styles.year}>
          {this.props.year}
        </VirtualTableRowCell>

        <VirtualTableRowCell className={styles.imdbId}>
          <Label>{this.props.imdbId}</Label>
        </VirtualTableRowCell>

        <VirtualTableRowCell className={styles.tmdbId}>
          <Label>{this.props.tmdbId}</Label>
        </VirtualTableRowCell>
      </>
    );
  }
}

SelectMovieRow.propTypes = {
  id: PropTypes.number.isRequired,
  title: PropTypes.string.isRequired,
  tmdbId: PropTypes.number.isRequired,
  imdbId: PropTypes.string.isRequired,
  year: PropTypes.number.isRequired,
  onMovieSelect: PropTypes.func.isRequired
};

export default SelectMovieRow;
