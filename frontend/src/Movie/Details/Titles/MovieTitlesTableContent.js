import PropTypes from 'prop-types';
import React, { Component } from 'react';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import styles from './MovieTitlesTableContent.css';
import MovieTitlesRow from './MovieTitlesRow';

const columns = [
  {
    name: 'altTitle',
    label: 'Alternative Title',
    isVisible: true
  },
  {
    name: 'language',
    label: 'Language',
    isVisible: true
  },
  {
    name: 'sourceType',
    label: 'Type',
    isVisible: true
  }
];

class MovieTitlesTableContent extends Component {

  //
  // Render

  render() {
    const {
      isFetching,
      isPopulated,
      error,
      items
    } = this.props;

    const hasItems = !!items.length;
    return (
      <div>
        {
          isFetching &&
            <LoadingIndicator />
        }

        {
          !isFetching && !!error &&
            <div className={styles.blankpad}>Unable to load alternative titles.</div>
        }

        {
          isPopulated && !hasItems && !error &&
            <div className={styles.blankpad}>No alternative titles.</div>
        }

        {
          isPopulated && hasItems && !error &&
            <Table columns={columns}>
              <TableBody>
                {
                  items.reverse().map((item) => {
                    return (
                      <MovieTitlesRow
                        key={item.id}
                        {...item}
                      />
                    );
                  })
                }
              </TableBody>
            </Table>
        }
      </div>
    );
  }
}

MovieTitlesTableContent.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired
};

export default MovieTitlesTableContent;
