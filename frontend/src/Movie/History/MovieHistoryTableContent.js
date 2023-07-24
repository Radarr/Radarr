import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Icon from 'Components/Icon';
import IconButton from 'Components/Link/IconButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import MovieHistoryRowConnector from './MovieHistoryRowConnector';
import styles from './MovieHistoryTableContent.css';

const columns = [
  {
    name: 'eventType',
    isVisible: true
  },
  {
    name: 'sourceTitle',
    get label() {
      return translate('SourceTitle');
    },
    isVisible: true
  },
  {
    name: 'languages',
    get label() {
      return translate('Languages');
    },
    isVisible: true
  },
  {
    name: 'quality',
    get label() {
      return translate('Quality');
    },
    isVisible: true
  },
  {
    name: 'customFormats',
    get label() {
      return translate('CustomFormats');
    },
    isSortable: false,
    isVisible: true
  },
  {
    name: 'customFormatScore',
    label: React.createElement(Icon, {
      name: icons.SCORE,
      title: 'Custom format score'
    }),
    isSortable: true,
    isVisible: true
  },
  {
    name: 'date',
    get label() {
      return translate('Date');
    },
    isVisible: true
  },
  {
    name: 'actions',
    label: React.createElement(IconButton, { name: icons.ADVANCED_SETTINGS }),
    isVisible: true
  }
];

class MovieHistoryTableContent extends Component {

  //
  // Render

  render() {
    const {
      isFetching,
      isPopulated,
      error,
      items,
      onMarkAsFailedPress
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
              {translate('UnableToLoadHistory')}
            </div>
        }

        {
          isPopulated && !hasItems && !error &&
            <div className={styles.blankpad}>
              {translate('NoHistory')}
            </div>
        }

        {
          isPopulated && hasItems && !error &&
            <Table columns={columns}>
              <TableBody>
                {
                  items.map((item) => {
                    return (
                      <MovieHistoryRowConnector
                        key={item.id}
                        {...item}
                        onMarkAsFailedPress={onMarkAsFailedPress}
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

MovieHistoryTableContent.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  onMarkAsFailedPress: PropTypes.func.isRequired
};

export default MovieHistoryTableContent;
