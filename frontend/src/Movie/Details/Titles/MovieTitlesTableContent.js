import PropTypes from 'prop-types';
import React, { Component } from 'react';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import translate from 'Utilities/String/translate';
import MovieTitlesRow from './MovieTitlesRow';
import styles from './MovieTitlesTableContent.css';

const columns = [
  {
    name: 'altTitle',
    get label() {
      return translate('AlternativeTitle');
    },
    isVisible: true
  },
  {
    name: 'language',
    get label() {
      return translate('Language');
    },
    isVisible: true
  },
  {
    name: 'sourceType',
    get label() {
      return translate('Type');
    },
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
            <div className={styles.blankpad}>
              {translate('UnableToLoadAltTitle')}
            </div>
        }

        {
          isPopulated && !hasItems && !error &&
            <div className={styles.blankpad}>
              {translate('NoAltTitle')}
            </div>
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
