/* eslint max-params: 0 */
import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import createArtistSelector from 'Store/Selectors/createArtistSelector';
import createCommandsSelector from 'Store/Selectors/createCommandsSelector';
// import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import { toggleAlbumsMonitored, setAlbumsTableOption } from 'Store/Actions/albumActions';
import { setSeriesSort } from 'Store/Actions/seriesActions';
import { executeCommand } from 'Store/Actions/commandActions';
import AuthorDetailsSeries from './AuthorDetailsSeries';

function createMapStateToProps() {
  return createSelector(
    (state, { seriesId }) => seriesId,
    (state) => state.albums,
    createArtistSelector(),
    (state) => state.series,
    createCommandsSelector(),
    createDimensionsSelector(),
    createUISettingsSelector(),
    (seriesId, books, author, series, commands, dimensions, uiSettings) => {

      const currentSeries = _.find(series.items, { id: seriesId });

      const bookIds = currentSeries.links.map((x) => x.bookId);
      const positionMap = currentSeries.links.reduce((acc, curr) => {
        acc[curr.bookId] = curr.position;
        return acc;
      }, {});

      const booksInSeries = _.filter(books.items, (book) => bookIds.includes(book.id));

      let sortDir = 'asc';

      if (series.sortDirection === 'descending') {
        sortDir = 'desc';
      }

      let sortedBooks = [];
      if (series.sortKey === 'position') {
        sortedBooks = booksInSeries.sort((a, b) => {
          const apos = positionMap[a.id] || '';
          const bpos = positionMap[b.id] || '';
          return apos.localeCompare(bpos, undefined, { numeric: true, sensivity: 'base' });
        });
      } else {
        sortedBooks = _.orderBy(booksInSeries, series.sortKey, sortDir);
      }

      return {
        id: currentSeries.id,
        label: currentSeries.title,
        items: sortedBooks,
        positionMap,
        columns: series.columns,
        sortKey: series.sortKey,
        sortDirection: series.sortDirection,
        artistMonitored: author.monitored,
        isSmallScreen: dimensions.isSmallScreen,
        uiSettings
      };
    }
  );
}

const mapDispatchToProps = {
  toggleAlbumsMonitored,
  setAlbumsTableOption,
  dispatchSetSeriesSort: setSeriesSort,
  executeCommand
};

class ArtistDetailsSeasonConnector extends Component {

  //
  // Listeners

  onTableOptionChange = (payload) => {
    this.props.setAlbumsTableOption(payload);
  }

  onSortPress = (sortKey) => {
    this.props.dispatchSetSeriesSort({ sortKey });
  }

  onMonitorAlbumPress = (bookIds, monitored) => {
    this.props.toggleAlbumsMonitored({
      bookIds,
      monitored
    });
  }

  //
  // Render

  render() {
    return (
      <AuthorDetailsSeries
        {...this.props}
        onSortPress={this.onSortPress}
        onTableOptionChange={this.onTableOptionChange}
        onMonitorAlbumPress={this.onMonitorAlbumPress}
      />
    );
  }
}

ArtistDetailsSeasonConnector.propTypes = {
  authorId: PropTypes.number.isRequired,
  toggleAlbumsMonitored: PropTypes.func.isRequired,
  setAlbumsTableOption: PropTypes.func.isRequired,
  dispatchSetSeriesSort: PropTypes.func.isRequired,
  executeCommand: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(ArtistDetailsSeasonConnector);
