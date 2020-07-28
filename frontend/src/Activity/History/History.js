import PropTypes from 'prop-types';
import React, { Component } from 'react';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import FilterMenu from 'Components/Menu/FilterMenu';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import TableOptionsModalWrapper from 'Components/Table/TableOptions/TableOptionsModalWrapper';
import TablePager from 'Components/Table/TablePager';
import { align, icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import HistoryRowConnector from './HistoryRowConnector';

class History extends Component {

  //
  // Render

  render() {
    const {
      isFetching,
      isPopulated,
      error,
      isMoviesFetching,
      isMoviesPopulated,
      moviesError,
      items,
      columns,
      selectedFilterKey,
      filters,
      totalRecords,
      onFilterSelect,
      onFirstPagePress,
      ...otherProps
    } = this.props;

    const isFetchingAny = isFetching || isMoviesFetching;
    const isAllPopulated = isPopulated && (isMoviesPopulated || !items.length);
    const hasError = error || moviesError;

    return (
      <PageContent title="History">
        <PageToolbar>
          <PageToolbarSection>
            <PageToolbarButton
              label={translate('Refresh')}
              iconName={icons.REFRESH}
              isSpinning={isFetching}
              onPress={onFirstPagePress}
            />
          </PageToolbarSection>

          <PageToolbarSection alignContent={align.RIGHT}>
            <TableOptionsModalWrapper
              {...otherProps}
              columns={columns}
            >
              <PageToolbarButton
                label={translate('Options')}
                iconName={icons.TABLE}
              />
            </TableOptionsModalWrapper>

            <FilterMenu
              alignMenu={align.RIGHT}
              selectedFilterKey={selectedFilterKey}
              filters={filters}
              customFilters={[]}
              onFilterSelect={onFilterSelect}
            />
          </PageToolbarSection>
        </PageToolbar>

        <PageContentBody>
          {
            isFetchingAny && !isAllPopulated &&
              <LoadingIndicator />
          }

          {
            !isFetchingAny && hasError &&
              <div>Unable to load history</div>
          }

          {
            // If history isPopulated and it's empty show no history found and don't
            // wait for the episodes to populate because they are never coming.

            isPopulated && !hasError && !items.length &&
              <div>
                No history found
              </div>
          }

          {
            isAllPopulated && !hasError && !!items.length &&
              <div>
                <Table
                  columns={columns}
                  {...otherProps}
                >
                  <TableBody>
                    {
                      items.map((item) => {
                        return (
                          <HistoryRowConnector
                            key={item.id}
                            columns={columns}
                            {...item}
                          />
                        );
                      })
                    }
                  </TableBody>
                </Table>

                <TablePager
                  totalRecords={totalRecords}
                  isFetching={isFetching}
                  onFirstPagePress={onFirstPagePress}
                  {...otherProps}
                />
              </div>
          }
        </PageContentBody>
      </PageContent>
    );
  }
}

History.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  isMoviesFetching: PropTypes.bool.isRequired,
  isMoviesPopulated: PropTypes.bool.isRequired,
  moviesError: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  selectedFilterKey: PropTypes.string.isRequired,
  filters: PropTypes.arrayOf(PropTypes.object).isRequired,
  totalRecords: PropTypes.number,
  onFilterSelect: PropTypes.func.isRequired,
  onFirstPagePress: PropTypes.func.isRequired
};

export default History;
