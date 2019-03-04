import PropTypes from 'prop-types';
import React, { Component } from 'react';
import TextTruncate from 'react-text-truncate';
import { icons } from 'Helpers/Props';
import dimensions from 'Styles/Variables/dimensions';
import fonts from 'Styles/Variables/fonts';
import IconButton from 'Components/Link/IconButton';
import Link from 'Components/Link/Link';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import ArtistPoster from 'Artist/ArtistPoster';
import EditArtistModalConnector from 'Artist/Edit/EditArtistModalConnector';
import DeleteArtistModal from 'Artist/Delete/DeleteArtistModal';
import ArtistIndexProgressBar from 'Artist/Index/ProgressBar/ArtistIndexProgressBar';
import ArtistIndexOverviewInfo from './ArtistIndexOverviewInfo';
import styles from './ArtistIndexOverview.css';

const columnPadding = parseInt(dimensions.artistIndexColumnPadding);
const columnPaddingSmallScreen = parseInt(dimensions.artistIndexColumnPaddingSmallScreen);
const defaultFontSize = parseInt(fonts.defaultFontSize);
const lineHeight = parseFloat(fonts.lineHeight);

// Hardcoded height beased on line-height of 32 + bottom margin of 10.
// Less side-effecty than using react-measure.
const titleRowHeight = 42;

function getContentHeight(rowHeight, isSmallScreen) {
  const padding = isSmallScreen ? columnPaddingSmallScreen : columnPadding;

  return rowHeight - padding;
}

class ArtistIndexOverview extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isEditArtistModalOpen: false,
      isDeleteArtistModalOpen: false
    };
  }

  //
  // Listeners

  onEditArtistPress = () => {
    this.setState({ isEditArtistModalOpen: true });
  }

  onEditArtistModalClose = () => {
    this.setState({ isEditArtistModalOpen: false });
  }

  onDeleteArtistPress = () => {
    this.setState({
      isEditArtistModalOpen: false,
      isDeleteArtistModalOpen: true
    });
  }

  onDeleteArtistModalClose = () => {
    this.setState({ isDeleteArtistModalOpen: false });
  }

  //
  // Render

  render() {
    const {
      style,
      id,
      artistName,
      overview,
      monitored,
      status,
      foreignArtistId,
      nextAiring,
      statistics,
      images,
      posterWidth,
      posterHeight,
      qualityProfile,
      overviewOptions,
      showSearchAction,
      showRelativeDates,
      shortDateFormat,
      longDateFormat,
      timeFormat,
      rowHeight,
      isSmallScreen,
      isRefreshingArtist,
      isSearchingArtist,
      onRefreshArtistPress,
      onSearchPress,
      ...otherProps
    } = this.props;

    const {
      albumCount,
      sizeOnDisk,
      trackCount,
      trackFileCount,
      totalTrackCount
    } = statistics;

    const {
      isEditArtistModalOpen,
      isDeleteArtistModalOpen
    } = this.state;

    const link = `/artist/${foreignArtistId}`;

    const elementStyle = {
      width: `${posterWidth}px`,
      height: `${posterHeight}px`
    };

    const contentHeight = getContentHeight(rowHeight, isSmallScreen);
    const overviewHeight = contentHeight - titleRowHeight;

    return (
      <div className={styles.container} style={style}>
        <div className={styles.content}>
          <div className={styles.poster}>
            <div className={styles.posterContainer}>
              {
                status === 'ended' &&
                <div
                  className={styles.ended}
                  title="Ended"
                />
              }

              <Link
                className={styles.link}
                style={elementStyle}
                to={link}
              >
                <ArtistPoster
                  className={styles.poster}
                  style={elementStyle}
                  images={images}
                  size={250}
                  lazy={false}
                  overflow={true}
                />
              </Link>
            </div>

            <ArtistIndexProgressBar
              monitored={monitored}
              status={status}
              trackCount={trackCount}
              trackFileCount={trackFileCount}
              totalTrackCount={totalTrackCount}
              posterWidth={posterWidth}
              detailedProgressBar={overviewOptions.detailedProgressBar}
            />
          </div>

          <div className={styles.info} style={{ maxHeight: contentHeight }}>
            <div className={styles.titleRow}>
              <Link
                className={styles.title}
                to={link}
              >
                {artistName}
              </Link>

              <div className={styles.actions}>
                <SpinnerIconButton
                  name={icons.REFRESH}
                  title="Refresh Artist"
                  isSpinning={isRefreshingArtist}
                  onPress={onRefreshArtistPress}
                />

                {
                  showSearchAction &&
                    <SpinnerIconButton
                      className={styles.action}
                      name={icons.SEARCH}
                      title="Search for monitored albums"
                      isSpinning={isSearchingArtist}
                      onPress={onSearchPress}
                    />
                }

                <IconButton
                  name={icons.EDIT}
                  title="Edit Artist"
                  onPress={this.onEditArtistPress}
                />
              </div>
            </div>

            <div className={styles.details}>

              <Link
                className={styles.overview}
                to={link}
              >
                <TextTruncate
                  line={Math.floor(overviewHeight / (defaultFontSize * lineHeight))}
                  text={overview}
                />
              </Link>

              <ArtistIndexOverviewInfo
                height={overviewHeight}
                monitored={monitored}
                nextAiring={nextAiring}
                albumCount={albumCount}
                sizeOnDisk={sizeOnDisk}
                qualityProfile={qualityProfile}
                showRelativeDates={showRelativeDates}
                shortDateFormat={shortDateFormat}
                longDateFormat={longDateFormat}
                timeFormat={timeFormat}
                {...overviewOptions}
                {...otherProps}
              />
            </div>
          </div>
        </div>

        <EditArtistModalConnector
          isOpen={isEditArtistModalOpen}
          artistId={id}
          onModalClose={this.onEditArtistModalClose}
          onDeleteArtistPress={this.onDeleteArtistPress}
        />

        <DeleteArtistModal
          isOpen={isDeleteArtistModalOpen}
          artistId={id}
          onModalClose={this.onDeleteArtistModalClose}
        />
      </div>
    );
  }
}

ArtistIndexOverview.propTypes = {
  style: PropTypes.object.isRequired,
  id: PropTypes.number.isRequired,
  artistName: PropTypes.string.isRequired,
  overview: PropTypes.string.isRequired,
  monitored: PropTypes.bool.isRequired,
  status: PropTypes.string.isRequired,
  foreignArtistId: PropTypes.string.isRequired,
  nextAiring: PropTypes.string,
  statistics: PropTypes.object.isRequired,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  posterWidth: PropTypes.number.isRequired,
  posterHeight: PropTypes.number.isRequired,
  rowHeight: PropTypes.number.isRequired,
  qualityProfile: PropTypes.object.isRequired,
  overviewOptions: PropTypes.object.isRequired,
  showSearchAction: PropTypes.bool.isRequired,
  showRelativeDates: PropTypes.bool.isRequired,
  shortDateFormat: PropTypes.string.isRequired,
  longDateFormat: PropTypes.string.isRequired,
  timeFormat: PropTypes.string.isRequired,
  isSmallScreen: PropTypes.bool.isRequired,
  isRefreshingArtist: PropTypes.bool.isRequired,
  isSearchingArtist: PropTypes.bool.isRequired,
  onRefreshArtistPress: PropTypes.func.isRequired,
  onSearchPress: PropTypes.func.isRequired
};

ArtistIndexOverview.defaultProps = {
  statistics: {
    albumCount: 0,
    trackCount: 0,
    trackFileCount: 0,
    totalTrackCount: 0
  }
};

export default ArtistIndexOverview;
