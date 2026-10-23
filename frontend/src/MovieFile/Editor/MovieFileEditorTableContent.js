import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import { sortDirections } from 'Helpers/Props';
import MovieVersionHeaderRow from 'Movie/Details/Versions/MovieVersionHeaderRow';
import translate from 'Utilities/String/translate';
import MovieFileEditorRow from './MovieFileEditorRow';
import styles from './MovieFileEditorTableContent.css';

function groupFilesByVersion(items, versions) {
  const primary = versions.find((version) => version.isPrimary) ?? versions[0];

  return versions.map((version) => {
    return {
      version,
      files: items.filter((item) => {
        const owner = versions.find((v) => v.movieFileId === item.id) ?? primary;

        return owner.id === version.id;
      })
    };
  });
}

class MovieFileEditorTableContent extends Component {

  //
  // Render

  renderRow = (item) => {
    return (
      <MovieFileEditorRow
        key={item.id}
        columns={this.props.columns}
        {...item}
        onDeletePress={this.props.onDeletePress}
      />
    );
  };

  render() {
    const {
      items,
      versions,
      columns,
      sortKey,
      sortDirection,
      onSortPress,
      onTableOptionChange,
      onRemoveVersionPress
    } = this.props;

    // With several versions, show each version's files under a header with its quality profile and path
    const groups = versions && versions.length > 1 ? groupFilesByVersion(items, versions) : null;

    return (
      <div>
        {
          !items.length && !groups &&
            <div className={styles.blankpad}>
              {translate('NoMovieFilesToManage')}
            </div>
        }

        {
          (!!items.length || !!groups) &&
            <Table
              columns={columns}
              sortKey={sortKey}
              sortDirection={sortDirection}
              onSortPress={onSortPress}
              onTableOptionChange={onTableOptionChange}
            >
              <TableBody>
                {
                  groups ?
                    groups.map(({ version, files }) => {
                      return [
                        <MovieVersionHeaderRow
                          key={`version-${version.id}`}
                          version={version}
                          colSpan={columns.filter((column) => column.isVisible).length}
                          onRemovePress={onRemoveVersionPress}
                        />,
                        ...files.map(this.renderRow)
                      ];
                    }) :
                    items.map(this.renderRow)
                }
              </TableBody>
            </Table>
        }

      </div>
    );
  }
}

MovieFileEditorTableContent.propTypes = {
  movieId: PropTypes.number,
  isDeleting: PropTypes.bool.isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  sortKey: PropTypes.string.isRequired,
  sortDirection: PropTypes.oneOf(sortDirections.all),
  onTableOptionChange: PropTypes.func.isRequired,
  onSortPress: PropTypes.func.isRequired,
  onDeletePress: PropTypes.func.isRequired,
  versions: PropTypes.arrayOf(PropTypes.object),
  onRemoveVersionPress: PropTypes.func
};

export default MovieFileEditorTableContent;
