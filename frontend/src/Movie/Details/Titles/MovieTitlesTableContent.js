import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import translate from 'Utilities/String/translate';
import MovieTitlesRow from './MovieTitlesRow';
import styles from './MovieTitlesTableContent.css';

const columns = [
  {
    name: 'altTitle',
    label: translate('Title'),
    isVisible: true
  },
  {
    name: 'language',
    label: translate('Language'),
    isVisible: true
  },
  {
    name: 'sourceType',
    label: translate('Type'),
    isVisible: true
  }
];

class MovieTitlesTableContent extends Component {

  //
  // Render

  render() {
    const {
      titles
    } = this.props;

    const hasItems = !!titles.length;
    return (
      <div>
        {
          !hasItems &&
            <div className={styles.blankpad}>
              {translate('NoAltTitle')}
            </div>
        }

        {
          hasItems &&
            <Table columns={columns}>
              <TableBody>
                {
                  titles.reverse().map((item) => {
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
  titles: PropTypes.arrayOf(PropTypes.object).isRequired
};

export default MovieTitlesTableContent;
