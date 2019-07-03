import PropTypes from 'prop-types';
import React, { Component } from 'react';
import TableRow from 'Components/Table/TableRow';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import MovieLanguage from 'Movie/MovieLanguage';

class MovieTitlesRow extends Component {

  //
  // Render

  render() {
    const {
      title,
      language
    } = this.props;

    // TODO - Fix languages to all take arrays
    const languages = [language];

    return (
      <TableRow>

        <TableRowCell>
          {title}
        </TableRowCell>

        <TableRowCell>
          <MovieLanguage
            languages={languages}
          />
        </TableRowCell>

      </TableRow>
    );
  }
}

MovieTitlesRow.propTypes = {
  id: PropTypes.number.isRequired,
  title: PropTypes.string.isRequired,
  language: PropTypes.object.isRequired
};

export default MovieTitlesRow;
