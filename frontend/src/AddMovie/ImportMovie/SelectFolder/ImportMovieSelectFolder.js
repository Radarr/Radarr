import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { icons, kinds, sizes } from 'Helpers/Props';
import Button from 'Components/Link/Button';
import FieldSet from 'Components/FieldSet';
import Icon from 'Components/Icon';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import FileBrowserModal from 'Components/FileBrowser/FileBrowserModal';
import PageContent from 'Components/Page/PageContent';
import PageContentBodyConnector from 'Components/Page/PageContentBodyConnector';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import ImportMovieRootFolderRowConnector from './ImportMovieRootFolderRowConnector';
import styles from './ImportMovieSelectFolder.css';

const rootFolderColumns = [
  {
    name: 'path',
    label: 'Path',
    isVisible: true
  },
  {
    name: 'freeSpace',
    label: 'Free Space',
    isVisible: true
  },
  {
    name: 'unmappedFolders',
    label: 'Unmapped Folders',
    isVisible: true
  },
  {
    name: 'actions',
    isVisible: true
  }
];

class ImportMovieSelectFolder extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isAddNewRootFolderModalOpen: false
    };
  }

  //
  // Lifecycle

  onAddNewRootFolderPress = () => {
    this.setState({ isAddNewRootFolderModalOpen: true });
  }

  onNewRootFolderSelect = ({ value }) => {
    this.props.onNewRootFolderSelect(value);
  }

  onAddRootFolderModalClose = () => {
    this.setState({ isAddNewRootFolderModalOpen: false });
  }

  //
  // Render

  render() {
    const {
      isWindows,
      isFetching,
      isPopulated,
      error,
      items
    } = this.props;

    return (
      <PageContent title="Import Series">
        <PageContentBodyConnector>
          {
            isFetching && !isPopulated &&
              <LoadingIndicator />
          }

          {
            !isFetching && !!error &&
              <div>Unable to load root folders</div>
          }

          {
            !error && isPopulated &&
              <div>
                <div className={styles.header}>
                  Import movies you already have
                </div>

                <div className={styles.tips}>
                  Some tips to ensure the import goes smoothly:
                  <ul>
                    <li className={styles.tip}>
                      Make sure your files include the quality in the name. eg. <span className={styles.code}>episode.s02e15.bluray.mkv</span>
                    </li>
                    <li className={styles.tip}>
                      Point Radarr to the folder containing all of your movies not a specific one. eg. <span className={styles.code}>"{isWindows ? 'C:\\movies' : '/movies'}"</span> and not <span className={styles.code}>"{isWindows ? 'C:\\movies\\the matrix' : '/movies/the matrix'}"</span>
                    </li>
                  </ul>
                </div>

                {
                  items.length > 0 ?
                    <div className={styles.recentFolders}>
                      <FieldSet legend="Recent Folders">
                        <Table
                          columns={rootFolderColumns}
                        >
                          <TableBody>
                            {
                              items.map((rootFolder) => {
                                return (
                                  <ImportMovieRootFolderRowConnector
                                    key={rootFolder.id}
                                    id={rootFolder.id}
                                    path={rootFolder.path}
                                    freeSpace={rootFolder.freeSpace}
                                    unmappedFolders={rootFolder.unmappedFolders}
                                  />
                                );
                              })
                            }
                          </TableBody>
                        </Table>
                      </FieldSet>

                      <Button
                        kind={kinds.PRIMARY}
                        size={sizes.LARGE}
                        onPress={this.onAddNewRootFolderPress}
                      >
                        <Icon
                          className={styles.importButtonIcon}
                          name={icons.DRIVE}
                        />
                        Choose another folder
                      </Button>
                    </div> :

                    <div className={styles.startImport}>
                      <Button
                        kind={kinds.PRIMARY}
                        size={sizes.LARGE}
                        onPress={this.onAddNewRootFolderPress}
                      >
                        <Icon
                          className={styles.importButtonIcon}
                          name={icons.DRIVE}
                        />
                        Start Import
                      </Button>
                    </div>
                }

                <FileBrowserModal
                  isOpen={this.state.isAddNewRootFolderModalOpen}
                  name="rootFolderPath"
                  value=""
                  onChange={this.onNewRootFolderSelect}
                  onModalClose={this.onAddRootFolderModalClose}
                />
              </div>
          }
        </PageContentBodyConnector>
      </PageContent>
    );
  }
}

ImportMovieSelectFolder.propTypes = {
  isWindows: PropTypes.bool.isRequired,
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  onNewRootFolderSelect: PropTypes.func.isRequired,
  onDeleteRootFolderPress: PropTypes.func.isRequired
};

export default ImportMovieSelectFolder;
