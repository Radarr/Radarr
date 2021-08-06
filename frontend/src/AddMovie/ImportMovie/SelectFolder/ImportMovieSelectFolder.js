import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Alert from 'Components/Alert';
import FieldSet from 'Components/FieldSet';
import FileBrowserModal from 'Components/FileBrowser/FileBrowserModal';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import { icons, kinds, sizes } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import ImportMovieRootFolderRowConnector from './ImportMovieRootFolderRowConnector';
import styles from './ImportMovieSelectFolder.css';

const rootFolderColumns = [
  {
    name: 'path',
    label: translate('Path'),
    isVisible: true
  },
  {
    name: 'freeSpace',
    label: translate('FreeSpace'),
    isVisible: true
  },
  {
    name: 'unmappedFolders',
    label: translate('UnmappedFolders'),
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
      isSaving,
      error,
      saveError,
      items
    } = this.props;

    const hasRootFolders = items.length > 0;

    return (
      <PageContent title={translate('ImportMovies')}>
        <PageContentBody>
          {
            isFetching && !isPopulated ?
              <LoadingIndicator /> :
              null
          }

          {
            !isFetching && error ?
              <div>
                {translate('UnableToLoadRootFolders')}
              </div> :
              null
          }

          {
            !error && isPopulated &&
              <div>
                <div className={styles.header}>
                  {translate('ImportHeader')}
                </div>

                <div className={styles.tips}>
                  {translate('ImportTipsMessage')}
                  <ul>
                    <li className={styles.tip} dangerouslySetInnerHTML={{ __html: translate('ImportIncludeQuality', ['<code>movie.2008.bluray.mkv</code>']) }} />
                    <li className={styles.tip} dangerouslySetInnerHTML={{ __html: translate('ImportRootPath', [`<code>${isWindows ? 'C:\\movies' : '/movies'}</code>`, `<code>${isWindows ? 'C:\\movies\\the matrix' : '/movies/the matrix'}</code>`]) }} />
                    <li className={styles.tip}>{translate('ImportNotForDownloads')}</li>
                  </ul>
                </div>

                {
                  hasRootFolders ?
                    <div className={styles.recentFolders}>
                      <FieldSet legend={translate('RecentFolders')}>
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
                    </div> :
                    null
                }

                {
                  !isSaving && saveError ?
                    <Alert
                      className={styles.addErrorAlert}
                      kind={kinds.DANGER}
                    >
                      {translate('UnableToAddRootFolder')}

                      <ul>
                        {
                          saveError.responseJSON.map((e, index) => {
                            return (
                              <li key={index}>
                                {e.errorMessage}
                              </li>
                            );
                          })
                        }
                      </ul>
                    </Alert> :
                    null
                }

                <div className={hasRootFolders ? undefined : styles.startImport}>
                  <Button
                    kind={kinds.PRIMARY}
                    size={sizes.LARGE}
                    onPress={this.onAddNewRootFolderPress}
                  >
                    <Icon
                      className={styles.importButtonIcon}
                      name={icons.DRIVE}
                    />
                    {
                      hasRootFolders ?
                        translate('ChooseAnotherFolder') :
                        translate('StartImport')
                    }
                  </Button>
                </div>

                <FileBrowserModal
                  isOpen={this.state.isAddNewRootFolderModalOpen}
                  name="rootFolderPath"
                  value=""
                  onChange={this.onNewRootFolderSelect}
                  onModalClose={this.onAddRootFolderModalClose}
                />
              </div>
          }
        </PageContentBody>
      </PageContent>
    );
  }
}

ImportMovieSelectFolder.propTypes = {
  isWindows: PropTypes.bool.isRequired,
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  isSaving: PropTypes.bool.isRequired,
  error: PropTypes.object,
  saveError: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  onNewRootFolderSelect: PropTypes.func.isRequired,
  onDeleteRootFolderPress: PropTypes.func.isRequired
};

export default ImportMovieSelectFolder;
