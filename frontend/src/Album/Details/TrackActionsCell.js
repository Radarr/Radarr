import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { icons } from 'Helpers/Props';
import IconButton from 'Components/Link/IconButton';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import styles from './TrackActionsCell.css';

class TrackActionsCell extends Component {

  //
  // Render

  render() {

    return (

      // TODO: Placeholder until we figure out what to show here.
      <TableRowCell className={styles.TrackActionsCell}>
        <IconButton
          name={icons.DELETE}
          title="Delete Track"
        />

      </TableRowCell>
    );
  }
}

TrackActionsCell.propTypes = {
  id: PropTypes.number.isRequired,
  albumId: PropTypes.number.isRequired
};

export default TrackActionsCell;
