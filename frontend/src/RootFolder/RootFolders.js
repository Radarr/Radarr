import PropTypes from 'prop-types';
import React from 'react';
import Alert from 'Components/Alert';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import { kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import RootFolderRowConnector from './RootFolderRowConnector';

const rootFolderColumns = [
  {
    name: 'path',
    get label() {
      return translate('Path');
    },
    isVisible: true
  },
  {
    name: 'freeSpace',
    get label() {
      return translate('FreeSpace');
    },
    isVisible: true
  },
  {
    name: 'unmappedFolders',
    get label() {
      return translate('UnmappedFolders');
    },
    isVisible: true
  },
  {
    name: 'actions',
    isVisible: true
  }
];

function RootFolders(props) {
  const {
    isFetching,
    isPopulated,
    error,
    items
  } = props;

  if (isFetching && !isPopulated) {
    return (
      <LoadingIndicator />
    );
  }

  if (!isFetching && !!error) {
    return (
      <Alert kind={kinds.DANGER}>
        {translate('UnableToLoadRootFolders')}
      </Alert>
    );
  }

  return (
    <Table
      columns={rootFolderColumns}
    >
      <TableBody>
        {
          items.map((rootFolder) => {
            return (
              <RootFolderRowConnector
                key={rootFolder.id}
                id={rootFolder.id}
                path={rootFolder.path}
                accessible={rootFolder.accessible}
                freeSpace={rootFolder.freeSpace}
                unmappedFolders={rootFolder.unmappedFolders}
              />
            );
          })
        }
      </TableBody>
    </Table>
  );
}

RootFolders.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired
};

export default RootFolders;
