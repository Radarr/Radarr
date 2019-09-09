import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import getToggledRange from 'Utilities/Table/getToggledRange';
import { icons, sortDirections } from 'Helpers/Props';
import Icon from 'Components/Icon';
import IconButton from 'Components/Link/IconButton';
import Link from 'Components/Link/Link';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import TrackFileEditorModal from 'TrackFile/Editor/TrackFileEditorModal';
import OrganizePreviewModalConnector from 'Organize/OrganizePreviewModalConnector';
import AlbumRowConnector from './AlbumRowConnector';
import styles from './ArtistDetailsSeason.css';

class ArtistDetailsSeason extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isOrganizeModalOpen: false,
      isManageTracksOpen: false,
      lastToggledAlbum: null
    };
  }

  componentDidMount() {
    this._expandByDefault();
  }

  componentDidUpdate(prevProps) {
    const {
      artistId
    } = this.props;

    if (prevProps.artistId !== artistId) {
      this._expandByDefault();
      return;
    }
  }

  //
  // Control

  _expandByDefault() {
    const {
      name,
      onExpandPress,
      items,
      uiSettings
    } = this.props;

    const expand = _.some(items, (item) =>
      ((item.albumType === 'Album') && uiSettings.expandAlbumByDefault) ||
        ((item.albumType === 'Single') && uiSettings.expandSingleByDefault) ||
        ((item.albumType === 'EP') && uiSettings.expandEPByDefault) ||
        ((item.albumType === 'Broadcast') && uiSettings.expandBroadcastByDefault) ||
        ((item.albumType === 'Other') && uiSettings.expandOtherByDefault));

    onExpandPress(name, expand);
  }

  //
  // Listeners

  onOrganizePress = () => {
    this.setState({ isOrganizeModalOpen: true });
  }

  onOrganizeModalClose = () => {
    this.setState({ isOrganizeModalOpen: false });
  }

  onManageTracksPress = () => {
    this.setState({ isManageTracksOpen: true });
  }

  onManageTracksModalClose = () => {
    this.setState({ isManageTracksOpen: false });
  }

  onExpandPress = () => {
    const {
      name,
      isExpanded
    } = this.props;

    this.props.onExpandPress(name, !isExpanded);
  }

  onMonitorAlbumPress = (albumId, monitored, { shiftKey }) => {
    const lastToggled = this.state.lastToggledAlbum;
    const albumIds = [albumId];

    if (shiftKey && lastToggled) {
      const { lower, upper } = getToggledRange(this.props.items, albumId, lastToggled);
      const items = this.props.items;

      for (let i = lower; i < upper; i++) {
        albumIds.push(items[i].id);
      }
    }

    this.setState({ lastToggledAlbum: albumId });

    this.props.onMonitorAlbumPress(_.uniq(albumIds), monitored);
  }

  //
  // Render

  render() {
    const {
      artistId,
      label,
      items,
      columns,
      isExpanded,
      sortKey,
      sortDirection,
      onSortPress,
      isSmallScreen,
      onTableOptionChange
    } = this.props;

    const {
      isOrganizeModalOpen,
      isManageTracksOpen
    } = this.state;

    return (
      <div
        className={styles.albumType}
      >
        <Link
          className={styles.expandButton}
          onPress={this.onExpandPress}
        >
          <div className={styles.header}>
            <div className={styles.left}>
              {
                <div>
                  <span className={styles.albumTypeLabel}>
                    {label}
                  </span>

                  <span className={styles.albumCount}>
                    ({items.length} Releases)
                  </span>
                </div>
              }

            </div>

            <Icon
              className={styles.expandButtonIcon}
              name={isExpanded ? icons.COLLAPSE : icons.EXPAND}
              title={isExpanded ? 'Hide albums' : 'Show albums'}
              size={24}
            />

            {
              !isSmallScreen &&
                <span>&nbsp;</span>
            }

          </div>
        </Link>

        <div>
          {
            isExpanded &&
              <div className={styles.albums}>
                {
                  items.length ?
                    <Table
                      columns={columns}
                      sortKey={sortKey}
                      sortDirection={sortDirection}
                      onSortPress={onSortPress}
                      onTableOptionChange={onTableOptionChange}
                    >
                      <TableBody>
                        {
                          items.map((item) => {
                            return (
                              <AlbumRowConnector
                                key={item.id}
                                columns={columns}
                                {...item}
                                onMonitorAlbumPress={this.onMonitorAlbumPress}
                              />
                            );
                          })
                        }
                      </TableBody>
                    </Table> :

                    <div className={styles.noAlbums}>
                      No releases in this group
                    </div>
                }
                <div className={styles.collapseButtonContainer}>
                  <IconButton
                    iconClassName={styles.collapseButtonIcon}
                    name={icons.COLLAPSE}
                    size={20}
                    title="Hide albums"
                    onPress={this.onExpandPress}
                  />
                </div>
              </div>
          }
        </div>

        <OrganizePreviewModalConnector
          isOpen={isOrganizeModalOpen}
          artistId={artistId}
          onModalClose={this.onOrganizeModalClose}
        />

        <TrackFileEditorModal
          isOpen={isManageTracksOpen}
          artistId={artistId}
          onModalClose={this.onManageTracksModalClose}
        />
      </div>
    );
  }
}

ArtistDetailsSeason.propTypes = {
  artistId: PropTypes.number.isRequired,
  name: PropTypes.string.isRequired,
  label: PropTypes.string.isRequired,
  sortKey: PropTypes.string,
  sortDirection: PropTypes.oneOf(sortDirections.all),
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  isExpanded: PropTypes.bool,
  isSmallScreen: PropTypes.bool.isRequired,
  onTableOptionChange: PropTypes.func.isRequired,
  onExpandPress: PropTypes.func.isRequired,
  onSortPress: PropTypes.func.isRequired,
  onMonitorAlbumPress: PropTypes.func.isRequired,
  uiSettings: PropTypes.object.isRequired
};

export default ArtistDetailsSeason;
