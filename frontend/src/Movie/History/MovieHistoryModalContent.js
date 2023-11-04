import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Alert from 'Components/Alert';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import { icons, kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import MovieHistoryRowConnector from './MovieHistoryRowConnector';

const columns = [
  {
    name: 'eventType',
    isVisible: true
  },
  {
    name: 'sourceTitle',
    label: () => translate('SourceTitle'),
    isVisible: true
  },
  {
    name: 'languages',
    label: () => translate('Languages'),
    isVisible: true
  },
  {
    name: 'quality',
    label: () => translate('Quality'),
    isVisible: true
  },
  {
    name: 'customFormats',
    label: () => translate('CustomFormats'),
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
    label: () => translate('Date'),
    isVisible: true
  },
  {
    name: 'actions',
    label: () => translate('Actions'),
    isVisible: true
  }
];

class MovieHistoryModalContent extends Component {

  //
  // Render

  render() {
    const {
      isFetching,
      isPopulated,
      error,
      items,
      onMarkAsFailedPress,
      onModalClose
    } = this.props;

    const hasItems = !!items.length;

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          {translate('History')}
        </ModalHeader>

        <ModalBody>
          {
            isFetching &&
              <LoadingIndicator />
          }

          {
            !isFetching && !!error &&
              <Alert kind={kinds.DANGER}>{translate('HistoryLoadError')}</Alert>
          }

          {
            isPopulated && !hasItems && !error &&
              <div>{translate('NoHistory')}</div>
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
        </ModalBody>

        <ModalFooter>
          <Button onPress={onModalClose}>
            {translate('Close')}
          </Button>
        </ModalFooter>
      </ModalContent>
    );
  }
}

MovieHistoryModalContent.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  onMarkAsFailedPress: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default MovieHistoryModalContent;
