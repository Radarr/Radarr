import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { Tab, Tabs, TabList, TabPanel } from 'react-tabs';
import albumEntities from 'Album/albumEntities';
import Button from 'Components/Link/Button';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';
import MonitorToggleButton from 'Components/MonitorToggleButton';
import AlbumHistoryConnector from './History/AlbumHistoryConnector';
import AlbumSearchConnector from './Search/AlbumSearchConnector';
import styles from './AlbumDetailsModalContent.css';

const tabs = [
  'history',
  'search'
];

class AlbumDetailsModalContent extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      selectedTab: props.selectedTab
    };
  }

  //
  // Listeners

  onTabSelect = (index, lastIndex) => {
    this.setState({ selectedTab: tabs[index] });
  }

  //
  // Render

  render() {
    const {
      albumId,
      artistName,
      foreignArtistId,
      artistMonitored,
      albumTitle,
      monitored,
      isSaving,
      showOpenArtistButton,
      startInteractiveSearch,
      onMonitorAlbumPress,
      onModalClose
    } = this.props;

    const artistLink = `/artist/${foreignArtistId}`;

    return (
      <ModalContent
        onModalClose={onModalClose}
      >
        <ModalHeader>
          <MonitorToggleButton
            className={styles.toggleButton}
            id={albumId}
            monitored={monitored}
            size={18}
            isDisabled={!artistMonitored}
            isSaving={isSaving}
            onPress={onMonitorAlbumPress}
          />

          <span className={styles.artistName}>
            {artistName}
          </span>

          <span className={styles.separator}>-</span>

          {albumTitle}
        </ModalHeader>

        <ModalBody>
          <Tabs
            className={styles.tabs}
            selectedIndex={tabs.indexOf(this.state.selectedTab)}
            onSelect={this.onTabSelect}
          >
            <TabList
              className={styles.tabList}
            >
              <Tab
                className={styles.tab}
                selectedClassName={styles.selectedTab}
              >
                History
              </Tab>

              <Tab
                className={styles.tab}
                selectedClassName={styles.selectedTab}
              >
                Search
              </Tab>
            </TabList>

            <TabPanel className={styles.tabPanel}>
              <AlbumHistoryConnector
                albumId={albumId}
              />
            </TabPanel>

            <TabPanel className={styles.tabPanel}>
              <AlbumSearchConnector
                albumId={albumId}
                startInteractiveSearch={startInteractiveSearch}
                onModalClose={onModalClose}
              />
            </TabPanel>
          </Tabs>
        </ModalBody>

        <ModalFooter>
          {
            showOpenArtistButton &&
              <Button
                className={styles.openArtistButton}
                to={artistLink}
                onPress={onModalClose}
              >
                Open Artist
              </Button>
          }

          <Button
            onPress={onModalClose}
          >
            Close
          </Button>
        </ModalFooter>
      </ModalContent>
    );
  }
}

AlbumDetailsModalContent.propTypes = {
  albumId: PropTypes.number.isRequired,
  albumEntity: PropTypes.string.isRequired,
  artistId: PropTypes.number.isRequired,
  artistName: PropTypes.string.isRequired,
  foreignArtistId: PropTypes.string.isRequired,
  artistMonitored: PropTypes.bool.isRequired,
  releaseDate: PropTypes.string.isRequired,
  albumLabel: PropTypes.arrayOf(PropTypes.string).isRequired,
  albumTitle: PropTypes.string.isRequired,
  monitored: PropTypes.bool.isRequired,
  isSaving: PropTypes.bool,
  showOpenArtistButton: PropTypes.bool,
  selectedTab: PropTypes.string.isRequired,
  startInteractiveSearch: PropTypes.bool.isRequired,
  onMonitorAlbumPress: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

AlbumDetailsModalContent.defaultProps = {
  selectedTab: 'history',
  albumLabel: ['Unknown'],
  albumEntity: albumEntities.ALBUMS,
  startInteractiveSearch: false
};

export default AlbumDetailsModalContent;
