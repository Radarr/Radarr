import classNames from 'classnames';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import AuthorBanner from 'Author/AuthorBanner';
import AuthorNameLink from 'Author/AuthorNameLink';
import DeleteAuthorModal from 'Author/Delete/DeleteAuthorModal';
import EditAuthorModalConnector from 'Author/Edit/EditAuthorModalConnector';
import BookTitleLink from 'Book/BookTitleLink';
import HeartRating from 'Components/HeartRating';
import IconButton from 'Components/Link/IconButton';
import Link from 'Components/Link/Link';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import ProgressBar from 'Components/ProgressBar';
import RelativeDateCellConnector from 'Components/Table/Cells/RelativeDateCellConnector';
import VirtualTableRowCell from 'Components/Table/Cells/VirtualTableRowCell';
import TagListConnector from 'Components/TagListConnector';
import { icons } from 'Helpers/Props';
import getProgressBarKind from 'Utilities/Author/getProgressBarKind';
import formatBytes from 'Utilities/Number/formatBytes';
import AuthorStatusCell from './AuthorStatusCell';
import hasGrowableColumns from './hasGrowableColumns';
import styles from './AuthorIndexRow.css';

class AuthorIndexRow extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      hasBannerError: false,
      isEditAuthorModalOpen: false,
      isDeleteAuthorModalOpen: false
    };
  }

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

  onUseSceneNumberingChange = () => {
    // Mock handler to satisfy `onChange` being required for `CheckInput`.
    //
  }

  onBannerLoad = () => {
    if (this.state.hasBannerError) {
      this.setState({ hasBannerError: false });
    }
  }

  onBannerLoadError = () => {
    if (!this.state.hasBannerError) {
      this.setState({ hasBannerError: true });
    }
  }

  //
  // Render

  render() {
    const {
      id,
      monitored,
      status,
      authorName,
      titleSlug,
      qualityProfile,
      metadataProfile,
      nextBook,
      lastBook,
      added,
      statistics,
      genres,
      ratings,
      path,
      tags,
      images,
      showBanners,
      showSearchAction,
      columns,
      isRefreshingAuthor,
      isSearchingAuthor,
      onRefreshAuthorPress,
      onSearchPress
    } = this.props;

    const {
      bookCount,
      bookFileCount,
      totalBookCount,
      sizeOnDisk
    } = statistics;

    const {
      hasBannerError,
      isEditAuthorModalOpen,
      isDeleteAuthorModalOpen
    } = this.state;

    return (
      <>
        {
          columns.map((column) => {
            const {
              name,
              isVisible
            } = column;

            if (!isVisible) {
              return null;
            }

            if (name === 'status') {
              return (
                <AuthorStatusCell
                  key={name}
                  className={styles[name]}
                  monitored={monitored}
                  status={status}
                  component={VirtualTableRowCell}
                />
              );
            }

            if (name === 'sortName') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={classNames(
                    styles[name],
                    showBanners && styles.banner,
                    showBanners && !hasGrowableColumns(columns) && styles.bannerGrow
                  )}
                >
                  {
                    showBanners ?
                      <Link
                        className={styles.link}
                        to={`/author/${titleSlug}`}
                      >
                        <AuthorBanner
                          className={styles.bannerImage}
                          images={images}
                          lazy={false}
                          overflow={true}
                          onError={this.onBannerLoadError}
                          onLoad={this.onBannerLoad}
                        />

                        {
                          hasBannerError &&
                            <div className={styles.overlayTitle}>
                              {authorName}
                            </div>
                        }
                      </Link> :

                      <AuthorNameLink
                        titleSlug={titleSlug}
                        authorName={authorName}
                      />
                  }
                </VirtualTableRowCell>
              );
            }

            if (name === 'qualityProfileId') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  {qualityProfile.name}
                </VirtualTableRowCell>
              );
            }

            if (name === 'metadataProfileId') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  {metadataProfile.name}
                </VirtualTableRowCell>
              );
            }

            if (name === 'nextBook') {
              if (nextBook) {
                return (
                  <VirtualTableRowCell
                    key={name}
                    className={styles[name]}
                  >
                    <BookTitleLink
                      title={nextBook.title}
                      disambiguation={nextBook.disambiguation}
                      titleSlug={nextBook.titleSlug}
                    />
                  </VirtualTableRowCell>
                );
              }
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  None
                </VirtualTableRowCell>
              );
            }

            if (name === 'lastBook') {
              if (lastBook) {
                return (
                  <VirtualTableRowCell
                    key={name}
                    className={styles[name]}
                  >
                    <BookTitleLink
                      title={lastBook.title}
                      disambiguation={lastBook.disambiguation}
                      titleSlug={lastBook.titleSlug}
                    />
                  </VirtualTableRowCell>
                );
              }
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  None
                </VirtualTableRowCell>
              );
            }

            if (name === 'added') {
              return (
                <RelativeDateCellConnector
                  key={name}
                  className={styles[name]}
                  date={added}
                  component={VirtualTableRowCell}
                />
              );
            }

            if (name === 'bookProgress') {
              const progress = bookCount ? bookFileCount / bookCount * 100 : 100;

              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  <ProgressBar
                    progress={progress}
                    kind={getProgressBarKind(status, monitored, progress)}
                    showText={true}
                    text={`${bookFileCount} / ${bookCount}`}
                    title={`${bookFileCount} / ${bookCount} (Total: ${totalBookCount})`}
                    width={125}
                  />
                </VirtualTableRowCell>
              );
            }

            if (name === 'path') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  {path}
                </VirtualTableRowCell>
              );
            }

            if (name === 'sizeOnDisk') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  {formatBytes(sizeOnDisk)}
                </VirtualTableRowCell>
              );
            }

            if (name === 'genres') {
              const joinedGenres = genres.join(', ');

              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  <span title={joinedGenres}>
                    {joinedGenres}
                  </span>
                </VirtualTableRowCell>
              );
            }

            if (name === 'ratings') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  <HeartRating
                    rating={ratings.value}
                  />
                </VirtualTableRowCell>
              );
            }

            if (name === 'tags') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  <TagListConnector
                    tags={tags}
                  />
                </VirtualTableRowCell>
              );
            }

            if (name === 'actions') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  <SpinnerIconButton
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
                    name={icons.EDIT}
                    title="Edit Author"
                    onPress={this.onEditAuthorPress}
                  />
                </VirtualTableRowCell>
              );
            }

            return null;
          })
        }

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
      </>
    );
  }
}

AuthorIndexRow.propTypes = {
  id: PropTypes.number.isRequired,
  monitored: PropTypes.bool.isRequired,
  status: PropTypes.string.isRequired,
  authorName: PropTypes.string.isRequired,
  titleSlug: PropTypes.string.isRequired,
  qualityProfile: PropTypes.object.isRequired,
  metadataProfile: PropTypes.object.isRequired,
  nextBook: PropTypes.object,
  lastBook: PropTypes.object,
  added: PropTypes.string,
  statistics: PropTypes.object.isRequired,
  latestBook: PropTypes.object,
  path: PropTypes.string.isRequired,
  genres: PropTypes.arrayOf(PropTypes.string).isRequired,
  ratings: PropTypes.object.isRequired,
  tags: PropTypes.arrayOf(PropTypes.number).isRequired,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  showBanners: PropTypes.bool.isRequired,
  showSearchAction: PropTypes.bool.isRequired,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  isRefreshingAuthor: PropTypes.bool.isRequired,
  isSearchingAuthor: PropTypes.bool.isRequired,
  onRefreshAuthorPress: PropTypes.func.isRequired,
  onSearchPress: PropTypes.func.isRequired
};

AuthorIndexRow.defaultProps = {
  statistics: {
    bookCount: 0,
    bookFileCount: 0,
    totalBookCount: 0
  },
  genres: [],
  tags: []
};

export default AuthorIndexRow;
