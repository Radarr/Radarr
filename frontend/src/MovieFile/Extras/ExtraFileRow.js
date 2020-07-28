import PropTypes from 'prop-types';
import React, { Component } from 'react';
import IconButton from 'Components/Link/IconButton';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableRow from 'Components/Table/TableRow';
import { icons } from 'Helpers/Props';
import titleCase from 'Utilities/String/titleCase';
import styles from './ExtraFileRow.css';

class ExtraFileRow extends Component {

  //
  // Render

  render() {
    const {
      relativePath,
      extension,
      type
    } = this.props;

    return (
      <TableRow>
        <TableRowCell
          className={styles.relativePath}
          title={relativePath}
        >
          {relativePath}
        </TableRowCell>

        <TableRowCell
          className={styles.extension}
          title={extension}
        >
          {extension}
        </TableRowCell>

        <TableRowCell
          className={styles.type}
          title={type}
        >
          {titleCase(type)}
        </TableRowCell>

        <TableRowCell className={styles.actions}>
          <IconButton
            name={icons.INFO}
          />
        </TableRowCell>
      </TableRow>
    );
  }

}

ExtraFileRow.propTypes = {
  id: PropTypes.number.isRequired,
  extension: PropTypes.string.isRequired,
  type: PropTypes.string.isRequired,
  relativePath: PropTypes.string.isRequired
};

export default ExtraFileRow;
