import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import TagListConnector from 'Components/TagListConnector';
import TableRow from 'Components/Table/TableRow';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableSelectCell from 'Components/Table/Cells/TableSelectCell';
import ArtistNameLink from 'Artist/ArtistNameLink';
import ArtistStatusCell from 'Artist/Index/Table/ArtistStatusCell';
import styles from './ArtistEditorRow.css';

class ArtistEditorRow extends Component {

  //
  // Listeners

  onAlbumFolderChange = () => {
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
      artistName,
      artistType,
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

        <ArtistStatusCell
          artistType={artistType}
          monitored={monitored}
          status={status}
        />

        <TableRowCell className={styles.title}>
          <ArtistNameLink
            titleSlug={titleSlug}
            artistName={artistName}
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

ArtistEditorRow.propTypes = {
  id: PropTypes.number.isRequired,
  status: PropTypes.string.isRequired,
  titleSlug: PropTypes.string.isRequired,
  artistName: PropTypes.string.isRequired,
  artistType: PropTypes.string,
  monitored: PropTypes.bool.isRequired,
  metadataProfile: PropTypes.object.isRequired,
  qualityProfile: PropTypes.object.isRequired,
  path: PropTypes.string.isRequired,
  tags: PropTypes.arrayOf(PropTypes.number).isRequired,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  isSelected: PropTypes.bool,
  onSelectedChange: PropTypes.func.isRequired
};

ArtistEditorRow.defaultProps = {
  tags: []
};

export default ArtistEditorRow;
