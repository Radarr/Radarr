import PropTypes from 'prop-types';
import React, { Component } from 'react';
import AuthorBanner from 'Author/AuthorBanner';
import DeleteAuthorModal from 'Author/Delete/DeleteAuthorModal';
import EditAuthorModalConnector from 'Author/Edit/EditAuthorModalConnector';
import AuthorIndexProgressBar from 'Author/Index/ProgressBar/AuthorIndexProgressBar';
import Label from 'Components/Label';
import IconButton from 'Components/Link/IconButton';
import Link from 'Components/Link/Link';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import { icons } from 'Helpers/Props';
import getRelativeDate from 'Utilities/Date/getRelativeDate';
import AuthorIndexBannerInfo from './AuthorIndexBannerInfo';
import styles from './AuthorIndexBanner.css';

class AuthorIndexBanner extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isEditAuthorModalOpen: false,
      isDeleteAuthorModalOpen: false
    };
  }

  //
  // Listeners

  onEditAuthorPress = () => {
    this.setState({ isEditAuthorModalOpen: true });
  }

  onEditAuthorModalClose = () => {
    this.setState({ isEditAuthorModalOpen: false });
  }

  onDeleteAuthorPress = () => {
    this.setState({
      isEditAuthorModalOpen: false,
      isDeleteAuthorModalOpen: true
    });
  }

  onDeleteAuthorModalClose = () => {
    this.setState({ isDeleteAuthorModalOpen: false });
  }

  //
  // Render

  render() {
    const {
      id,
      authorName,
      monitored,
      status,
      titleSlug,
      nextAiring,
      statistics,
      images,
      bannerWidth,
      bannerHeight,
      detailedProgressBar,
      showTitle,
      showMonitored,
      showQualityProfile,
      showSearchAction,
      qualityProfile,
      showRelativeDates,
      shortDateFormat,
      timeFormat,
      isRefreshingAuthor,
      isSearchingAuthor,
      onRefreshAuthorPress,
      onSearchPress,
      ...otherProps
    } = this.props;

    const {
      bookCount,
      sizeOnDisk,
      bookFileCount,
      totalBookCount
    } = statistics;

    const {
      isEditAuthorModalOpen,
      isDeleteAuthorModalOpen
    } = this.state;

    const link = `/author/${titleSlug}`;

    const elementStyle = {
      width: `${bannerWidth}px`,
      height: `${bannerHeight}px`
    };

    return (
      <div className={styles.container}>
        <div className={styles.content}>
          <div className={styles.bannerContainer}>
            <Label className={styles.controls}>
              <SpinnerIconButton
                className={styles.action}
                name={icons.REFRESH}
                title="Refresh Author"
                isSpinning={isRefreshingAuthor}
                onPress={onRefreshAuthorPress}
              />

              {
                showSearchAction &&
                  <SpinnerIconButton
                    className={styles.action}
                    name={icons.SEARCH}
                    title="Search for monitored books"
                    isSpinning={isSearchingAuthor}
                    onPress={onSearchPress}
                  />
              }

              <IconButton
                className={styles.action}
                name={icons.EDIT}
                title="Edit Author"
                onPress={this.onEditAuthorPress}
              />
            </Label>

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
              <AuthorBanner
                className={styles.banner}
                style={elementStyle}
                images={images}
                size={70}
                lazy={false}
                overflow={true}
              />
            </Link>
          </div>

          <AuthorIndexProgressBar
            monitored={monitored}
            status={status}
            bookCount={bookCount}
            bookFileCount={bookFileCount}
            totalBookCount={totalBookCount}
            posterWidth={bannerWidth}
            detailedProgressBar={detailedProgressBar}
          />

          {
            showTitle &&
              <div className={styles.title}>
                {authorName}
              </div>
          }

          {
            showMonitored &&
              <div className={styles.title}>
                {monitored ? 'Monitored' : 'Unmonitored'}
              </div>
          }

          {
            showQualityProfile &&
              <div className={styles.title}>
                {qualityProfile.name}
              </div>
          }
          {
            nextAiring &&
              <div className={styles.nextAiring}>
                {
                  getRelativeDate(
                    nextAiring,
                    shortDateFormat,
                    showRelativeDates,
                    {
                      timeFormat,
                      timeForToday: true
                    }
                  )
                }
              </div>
          }

          <AuthorIndexBannerInfo
            bookCount={bookCount}
            sizeOnDisk={sizeOnDisk}
            qualityProfile={qualityProfile}
            showQualityProfile={showQualityProfile}
            showRelativeDates={showRelativeDates}
            shortDateFormat={shortDateFormat}
            timeFormat={timeFormat}
            {...otherProps}
          />

          <EditAuthorModalConnector
            isOpen={isEditAuthorModalOpen}
            authorId={id}
            onModalClose={this.onEditAuthorModalClose}
            onDeleteAuthorPress={this.onDeleteAuthorPress}
          />

          <DeleteAuthorModal
            isOpen={isDeleteAuthorModalOpen}
            authorId={id}
            onModalClose={this.onDeleteAuthorModalClose}
          />
        </div>
      </div>
    );
  }
}

AuthorIndexBanner.propTypes = {
  id: PropTypes.number.isRequired,
  authorName: PropTypes.string.isRequired,
  monitored: PropTypes.bool.isRequired,
  status: PropTypes.string.isRequired,
  titleSlug: PropTypes.string.isRequired,
  nextAiring: PropTypes.string,
  statistics: PropTypes.object.isRequired,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  bannerWidth: PropTypes.number.isRequired,
  bannerHeight: PropTypes.number.isRequired,
  detailedProgressBar: PropTypes.bool.isRequired,
  showTitle: PropTypes.bool.isRequired,
  showMonitored: PropTypes.bool.isRequired,
  showQualityProfile: PropTypes.bool.isRequired,
  qualityProfile: PropTypes.object.isRequired,
  showSearchAction: PropTypes.bool.isRequired,
  showRelativeDates: PropTypes.bool.isRequired,
  shortDateFormat: PropTypes.string.isRequired,
  timeFormat: PropTypes.string.isRequired,
  isRefreshingAuthor: PropTypes.bool.isRequired,
  isSearchingAuthor: PropTypes.bool.isRequired,
  onRefreshAuthorPress: PropTypes.func.isRequired,
  onSearchPress: PropTypes.func.isRequired
};

AuthorIndexBanner.defaultProps = {
  statistics: {
    bookCount: 0,
    bookFileCount: 0,
    totalBookCount: 0
  }
};

export default AuthorIndexBanner;
