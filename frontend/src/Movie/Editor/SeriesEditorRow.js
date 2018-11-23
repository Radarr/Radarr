// import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
// import titleCase from 'Utilities/String/titleCase';
import TagListConnector from 'Components/TagListConnector';
// import CheckInput from 'Components/Form/CheckInput';
import TableRow from 'Components/Table/TableRow';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableSelectCell from 'Components/Table/Cells/TableSelectCell';
import MovieTitleLink from 'Movie/MovieTitleLink';
import MovieStatusCell from 'Movie/Index/Table/MovieStatusCell';

class SeriesEditorRow extends Component {

  //
  // Listeners

  onSeasonFolderChange = () => {
    // Mock handler to satisfy `onChange` being required for `CheckInput`.
    //
  }

  //
  // Render

  render() {
    const {
      id,
      status,
      titleSlug,
      title,
      monitored,
      qualityProfile,
      path,
      tags,
      // columns,
      isSelected,
      onSelectedChange
    } = this.props;

    return (
      <TableRow>
        <TableSelectCell
          id={id}
          isSelected={isSelected}
          onSelectedChange={onSelectedChange}
        />

        <MovieStatusCell
          monitored={monitored}
          status={status}
        />

        <TableRowCell>
          <MovieTitleLink
            titleSlug={titleSlug}
            title={title}
          />
        </TableRowCell>

        <TableRowCell>
          {qualityProfile.name}
        </TableRowCell>

        <TableRowCell>
          {path}
        </TableRowCell>

        <TableRowCell>
          <TagListConnector
            tags={tags}
          />
        </TableRowCell>
      </TableRow>
    );
  }
}

SeriesEditorRow.propTypes = {
  id: PropTypes.number.isRequired,
  status: PropTypes.string.isRequired,
  titleSlug: PropTypes.string.isRequired,
  title: PropTypes.string.isRequired,
  monitored: PropTypes.bool.isRequired,
  qualityProfile: PropTypes.object.isRequired,
  path: PropTypes.string.isRequired,
  tags: PropTypes.arrayOf(PropTypes.number).isRequired,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  isSelected: PropTypes.bool,
  onSelectedChange: PropTypes.func.isRequired
};

SeriesEditorRow.defaultProps = {
  tags: []
};

export default SeriesEditorRow;
