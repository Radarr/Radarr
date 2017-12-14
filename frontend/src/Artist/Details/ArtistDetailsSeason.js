import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import isAfter from 'Utilities/Date/isAfter';
import isBefore from 'Utilities/Date/isBefore';
import getToggledRange from 'Utilities/Table/getToggledRange';
import { align, icons, kinds, sizes } from 'Helpers/Props';
import Icon from 'Components/Icon';
import IconButton from 'Components/Link/IconButton';
import Link from 'Components/Link/Link';
import SpinnerIcon from 'Components/SpinnerIcon';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import Menu from 'Components/Menu/Menu';
import MenuButton from 'Components/Menu/MenuButton';
import MenuContent from 'Components/Menu/MenuContent';
import MenuItem from 'Components/Menu/MenuItem';
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
      isManageEpisodesOpen: false,
      lastToggledEpisode: null
    };
  }

  componentDidMount() {
    this._expandByDefault();
  }

  componentDidUpdate(prevProps) {
    if (prevProps.artistId !== this.props.artistId) {
      this._expandByDefault();
    }
  }

  //
  // Control

  _expandByDefault() {
    const {
      name,
      onExpandPress,
      items
    } = this.props;

    const expand = _.some(items, (item) => {
      return isAfter(item.releaseDate) ||
             isAfter(item.releaseDate, { days: -30 });
    });

    onExpandPress(name, expand && name > 0);
  }

  //
  // Listeners

  onOrganizePress = () => {
    this.setState({ isOrganizeModalOpen: true });
  }

  onOrganizeModalClose = () => {
    this.setState({ isOrganizeModalOpen: false });
  }

  onManageEpisodesPress = () => {
    this.setState({ isManageEpisodesOpen: true });
  }

  onManageEpisodesModalClose = () => {
    this.setState({ isManageEpisodesOpen: false });
  }

  onExpandPress = () => {
    const {
      name,
      isExpanded
    } = this.props;

    this.props.onExpandPress(name, !isExpanded);
  }

  onMonitorAlbumPress = (albumId, monitored, { shiftKey }) => {
    const lastToggled = this.state.lastToggledEpisode;
    const albumIds = [albumId];

    if (shiftKey && lastToggled) {
      const { lower, upper } = getToggledRange(this.props.items, albumId, lastToggled);
      const items = this.props.items;

      for (let i = lower; i < upper; i++) {
        albumIds.push(items[i].id);
      }
    }

    this.setState({ lastToggledEpisode: albumId });

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
      isSaving,
      isExpanded,
      isSearching,
      artistMonitored,
      isSmallScreen,
      onTableOptionChange,
      onSearchPress
    } = this.props;

    const {
      isOrganizeModalOpen,
      isManageEpisodesOpen
    } = this.state;

    return (
      <div
        className={styles.albumType}
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

          <Link
            className={styles.expandButton}
            onPress={this.onExpandPress}
          >

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
          </Link>

        </div>

        <div>
          {
            isExpanded &&
              <div className={styles.episodes}>
                {
                  items.length ?
                    <Table
                      columns={columns}
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

                    <div className={styles.noEpisodes}>
                      No albums in this group
                    </div>
                }
                <div className={styles.collapseButtonContainer}>
                  <IconButton
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
          isOpen={isManageEpisodesOpen}
          artistId={artistId}
          onModalClose={this.onManageEpisodesModalClose}
        />
      </div>
    );
  }
}

ArtistDetailsSeason.propTypes = {
  artistId: PropTypes.number.isRequired,
  name: PropTypes.string.isRequired,
  label: PropTypes.string.isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  isSaving: PropTypes.bool,
  isExpanded: PropTypes.bool,
  isSearching: PropTypes.bool.isRequired,
  artistMonitored: PropTypes.bool.isRequired,
  isSmallScreen: PropTypes.bool.isRequired,
  onTableOptionChange: PropTypes.func.isRequired,
  onExpandPress: PropTypes.func.isRequired,
  onMonitorAlbumPress: PropTypes.func.isRequired,
  onSearchPress: PropTypes.func.isRequired
};

export default ArtistDetailsSeason;
