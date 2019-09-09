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
import RootFolders from 'RootFolder/RootFolders';
import styles from './ImportArtistSelectFolder.css';

class ImportArtistSelectFolder extends Component {

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
      <PageContent title="Import Artist">
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
                  Import artist(s) you already have
                </div>

                <div className={styles.tips}>
                  Some tips to ensure the import goes smoothly:
                  <ul>
                    <li className={styles.tip}>
                    Point Lidarr to the folder containing all of your music not a specific one. eg. <span className={styles.code}>"{isWindows ? 'C:\\music' : '/music'}"</span> and not <span className={styles.code}>"{isWindows ? 'C:\\music\\sublime' : '/music/sublime'}"</span>
                    </li>
                  </ul>
                </div>

                {
                  items.length > 0 ?
                    <div className={styles.recentFolders}>
                      <FieldSet legend="Root Folders">
                        <RootFolders
                          isFetching={isFetching}
                          isPopulated={isPopulated}
                          error={error}
                          items={items}
                        />
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

ImportArtistSelectFolder.propTypes = {
  isWindows: PropTypes.bool.isRequired,
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  onNewRootFolderSelect: PropTypes.func.isRequired
};

export default ImportArtistSelectFolder;
