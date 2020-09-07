import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import AuthorNameLink from 'Author/AuthorNameLink';
import AuthorStatusCell from 'Author/Index/Table/AuthorStatusCell';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableSelectCell from 'Components/Table/Cells/TableSelectCell';
import TableRow from 'Components/Table/TableRow';
import TagListConnector from 'Components/TagListConnector';

class AuthorEditorRow extends Component {

  //
  // Listeners

  onBookFolderChange = () => {
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
      authorName,
      authorType,
      monitored,
      metadataProfile,
      qualityProfile,
      path,
      tags,
      columns,
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

        <AuthorStatusCell
          authorType={authorType}
          monitored={monitored}
          status={status}
        />

        <TableRowCell>
          <AuthorNameLink
            titleSlug={titleSlug}
            authorName={authorName}
          />
        </TableRowCell>

        <TableRowCell>
          {qualityProfile.name}
        </TableRowCell>

        {
          _.find(columns, { name: 'metadataProfileId' }).isVisible &&
            <TableRowCell>
              {metadataProfile.name}
            </TableRowCell>
        }

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

AuthorEditorRow.propTypes = {
  id: PropTypes.number.isRequired,
  status: PropTypes.string.isRequired,
  titleSlug: PropTypes.string.isRequired,
  authorName: PropTypes.string.isRequired,
  authorType: PropTypes.string,
  monitored: PropTypes.bool.isRequired,
  metadataProfile: PropTypes.object.isRequired,
  qualityProfile: PropTypes.object.isRequired,
  path: PropTypes.string.isRequired,
  tags: PropTypes.arrayOf(PropTypes.number).isRequired,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  isSelected: PropTypes.bool,
  onSelectedChange: PropTypes.func.isRequired
};

AuthorEditorRow.defaultProps = {
  tags: []
};

export default AuthorEditorRow;
