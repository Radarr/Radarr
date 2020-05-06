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
import AlbumRowConnector from './AlbumRowConnector';
import styles from './AuthorDetailsSeries.css';

class AuthorDetailsSeries extends Component {

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
      authorId
    } = this.props;

    if (prevProps.authorId !== authorId) {
      this._expandByDefault();
      return;
    }
  }

  //
  // Control

  _expandByDefault() {
    const {
      id,
      onExpandPress
    } = this.props;

    onExpandPress(id, true);
  }

  //
  // Listeners

  onExpandPress = () => {
    const {
      id,
      isExpanded
    } = this.props;

    this.props.onExpandPress(id, !isExpanded);
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
      label,
      items,
      positionMap,
      columns,
      isExpanded,
      sortKey,
      sortDirection,
      onSortPress,
      isSmallScreen,
      onTableOptionChange
    } = this.props;

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
                    ({items.length} Books)
                  </span>
                </div>
              }

            </div>

            <Icon
              className={styles.expandButtonIcon}
              name={isExpanded ? icons.COLLAPSE : icons.EXPAND}
              title={isExpanded ? 'Hide books' : 'Show books'}
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
                            position={positionMap[item.id]}
                            onMonitorAlbumPress={this.onMonitorAlbumPress}
                          />
                        );
                      })
                    }
                  </TableBody>
                </Table>

                <div className={styles.collapseButtonContainer}>
                  <IconButton
                    iconClassName={styles.collapseButtonIcon}
                    name={icons.COLLAPSE}
                    size={20}
                    title="Hide books"
                    onPress={this.onExpandPress}
                  />
                </div>
              </div>
          }
        </div>
      </div>
    );
  }
}

AuthorDetailsSeries.propTypes = {
  id: PropTypes.number.isRequired,
  authorId: PropTypes.number.isRequired,
  label: PropTypes.string.isRequired,
  sortKey: PropTypes.string,
  sortDirection: PropTypes.oneOf(sortDirections.all),
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  positionMap: PropTypes.object.isRequired,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  isExpanded: PropTypes.bool,
  isSmallScreen: PropTypes.bool.isRequired,
  onTableOptionChange: PropTypes.func.isRequired,
  onExpandPress: PropTypes.func.isRequired,
  onSortPress: PropTypes.func.isRequired,
  onMonitorAlbumPress: PropTypes.func.isRequired,
  uiSettings: PropTypes.object.isRequired
};

export default AuthorDetailsSeries;
