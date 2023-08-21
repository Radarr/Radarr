import PropTypes from 'prop-types';
import React, { Component } from 'react';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableRow from 'Components/Table/TableRow';
import titleCase from 'Utilities/String/titleCase';

class MovieTitlesRow extends Component {

  //
  // Render

  render() {
    const {
      title,
      sourceType
    } = this.props;

    return (
      <TableRow>

        <TableRowCell>
          {title}
        </TableRowCell>

        <TableRowCell>
          {titleCase(sourceType)}
        </TableRowCell>

      </TableRow>
    );
  }
}

MovieTitlesRow.propTypes = {
  id: PropTypes.number.isRequired,
  title: PropTypes.string.isRequired,
  sourceType: PropTypes.string.isRequired
};

export default MovieTitlesRow;
