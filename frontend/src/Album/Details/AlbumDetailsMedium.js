import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { icons, kinds, sizes } from 'Helpers/Props';
import Icon from 'Components/Icon';
import IconButton from 'Components/Link/IconButton';
import Label from 'Components/Label';
import Link from 'Components/Link/Link';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import TrackRowConnector from './TrackRowConnector';
import styles from './AlbumDetailsMedium.css';

function getMediumStatistics(tracks) {
  let trackCount = 0;
  let trackFileCount = 0;
  let totalTrackCount = 0;

  tracks.forEach((track) => {
    if (track.trackFileId) {
      trackCount++;
      trackFileCount++;
    } else {
      trackCount++;
    }

    totalTrackCount++;
  });

  return {
    trackCount,
    trackFileCount,
    totalTrackCount
  };
}

function getTrackCountKind(monitored, trackFileCount, trackCount) {
  if (trackFileCount === trackCount && trackCount > 0) {
    return kinds.SUCCESS;
  }

  if (!monitored) {
    return kinds.WARNING;
  }

  return kinds.DANGER;
}

class AlbumDetailsMedium extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this._expandByDefault();
  }

  componentDidUpdate(prevProps) {
    if (prevProps.albumId !== this.props.albumId) {
      this._expandByDefault();
    }
  }

  //
  // Control

  _expandByDefault() {
    const {
      mediumNumber,
      onExpandPress
    } = this.props;

    onExpandPress(mediumNumber, mediumNumber === 1);
  }

  //
  // Listeners

  onExpandPress = () => {
    const {
      mediumNumber,
      isExpanded
    } = this.props;

    this.props.onExpandPress(mediumNumber, !isExpanded);
  }

  //
  // Render

  render() {
    const {
      mediumNumber,
      mediumFormat,
      albumMonitored,
      items,
      columns,
      onTableOptionChange,
      isExpanded,
      isSmallScreen
    } = this.props;

    const {
      trackCount,
      trackFileCount,
      totalTrackCount
    } = getMediumStatistics(items);

    return (
      <div
        className={styles.medium}
      >
        <div className={styles.header}>
          <div className={styles.left}>
            {
              <div>
                <span className={styles.mediumNumber}>
                  {mediumFormat} {mediumNumber}
                </span>
              </div>
            }

            <Label
              title={`${totalTrackCount} tracks total. ${trackFileCount} tracks with files.`}
              kind={getTrackCountKind(albumMonitored, trackFileCount, trackCount)}
              size={sizes.LARGE}
            >
              {
                <span>{trackFileCount} / {trackCount}</span>
              }
            </Label>
          </div>

          <Link
            className={styles.expandButton}
            onPress={this.onExpandPress}
          >
            <Icon
              className={styles.expandButtonIcon}
              name={isExpanded ? icons.COLLAPSE : icons.EXPAND}
              title={isExpanded ? 'Hide tracks' : 'Show tracks'}
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
              <div className={styles.tracks}>
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
                              <TrackRowConnector
                                key={item.id}
                                columns={columns}
                                {...item}
                              />
                            );
                          })
                        }
                      </TableBody>
                    </Table> :

                    <div className={styles.noTracks}>
                      No tracks in this medium
                    </div>
                }
                <div className={styles.collapseButtonContainer}>
                  <IconButton
                    name={icons.COLLAPSE}
                    size={20}
                    title="Hide tracks"
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

AlbumDetailsMedium.propTypes = {
  albumId: PropTypes.number.isRequired,
  albumMonitored: PropTypes.bool.isRequired,
  mediumNumber: PropTypes.number.isRequired,
  mediumFormat: PropTypes.string.isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  isSaving: PropTypes.bool,
  isExpanded: PropTypes.bool,
  isSmallScreen: PropTypes.bool.isRequired,
  onTableOptionChange: PropTypes.func.isRequired,
  onExpandPress: PropTypes.func.isRequired
};

export default AlbumDetailsMedium;
