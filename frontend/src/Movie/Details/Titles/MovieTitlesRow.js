import PropTypes from 'prop-types';
import React, { Component } from 'react';
import titleCase from 'Utilities/String/titleCase';
import TableRow from 'Components/Table/TableRow';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import MovieLanguage from 'Movie/MovieLanguage';

class MovieTitlesRow extends Component {

  //
  // Render

  render() {
    const {
      title,
      language,
      sourceType
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
  language: PropTypes.object.isRequired,
  sourceType: PropTypes.object.isRequired
};

export default MovieTitlesRow;
