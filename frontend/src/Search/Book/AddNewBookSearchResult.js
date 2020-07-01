import moment from 'moment';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import TextTruncate from 'react-text-truncate';
import dimensions from 'Styles/Variables/dimensions';
import fonts from 'Styles/Variables/fonts';
import stripHtml from 'Utilities/String/stripHtml';
import { icons, sizes } from 'Helpers/Props';
import HeartRating from 'Components/HeartRating';
import Icon from 'Components/Icon';
import Label from 'Components/Label';
import Link from 'Components/Link/Link';
import BookCover from 'Book/BookCover';
import AddNewBookModal from './AddNewBookModal';
import styles from './AddNewBookSearchResult.css';

const columnPadding = parseInt(dimensions.authorIndexColumnPadding);
const columnPaddingSmallScreen = parseInt(dimensions.authorIndexColumnPaddingSmallScreen);
const defaultFontSize = parseInt(fonts.defaultFontSize);
const lineHeight = parseFloat(fonts.lineHeight);

function calculateHeight(rowHeight, isSmallScreen) {
  let height = rowHeight - 70;

  if (isSmallScreen) {
    height -= columnPaddingSmallScreen;
  } else {
    height -= columnPadding;
  }

  return height;
}

class AddNewBookSearchResult extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isNewAddBookModalOpen: false
    };
  }

  componentDidUpdate(prevProps) {
    if (!prevProps.isExistingBook && this.props.isExistingBook) {
      this.onAddBookModalClose();
    }
  }

  //
  // Listeners

  onPress = () => {
    this.setState({ isNewAddBookModalOpen: true });
  }

  onAddBookModalClose = () => {
    this.setState({ isNewAddBookModalOpen: false });
  }

  onMBLinkPress = (event) => {
    event.stopPropagation();
  }

  //
  // Render

  render() {
    const {
      foreignBookId,
      titleSlug,
      title,
      releaseDate,
      disambiguation,
      overview,
      ratings,
      images,
      author,
      editions,
      isExistingBook,
      isExistingAuthor,
      isSmallScreen
    } = this.props;

    const {
      isNewAddBookModalOpen
    } = this.state;

    const linkProps = isExistingBook ? { to: `/book/${titleSlug}` } : { onPress: this.onPress };

    const height = calculateHeight(230, isSmallScreen);

    return (
      <div className={styles.searchResult}>
        <Link
          className={styles.underlay}
          {...linkProps}
        />

        <div className={styles.overlay}>
          {
            !isSmallScreen &&
              <BookCover
                className={styles.poster}
                images={images}
                size={250}
                lazy={false}
              />
          }

          <div className={styles.content}>
            <div className={styles.name}>
              {title}

              {
                !!disambiguation &&
                  <span className={styles.year}>({disambiguation})</span>
              }

              {
                isExistingBook ?
                  <Icon
                    className={styles.alreadyExistsIcon}
                    name={icons.CHECK_CIRCLE}
                    size={20}
                    title="Book already in your library"
                  /> :
                  null
              }

              <Link
                className={styles.mbLink}
                to={`https://goodreads.com/book/show/${editions[0].foreignEditionId}`}
                onPress={this.onMBLinkPress}
              >
                <Icon
                  className={styles.mbLinkIcon}
                  name={icons.EXTERNAL_LINK}
                  size={28}
                />
              </Link>

            </div>

            <div>
              <span className={styles.authorName}> By: {author.authorName}</span>

              {
                isExistingAuthor ?
                  <Icon
                    className={styles.alreadyExistsIcon}
                    name={icons.CHECK_CIRCLE}
                    size={15}
                    title="Author already in your library"
                  /> :
                  null
              }
            </div>

            <div>
              <Label size={sizes.LARGE}>
                <HeartRating
                  rating={ratings.value}
                  iconSize={13}
                />
              </Label>

              {
                !!releaseDate &&
                  <Label size={sizes.LARGE}>
                    {moment(releaseDate).format('YYYY')}
                  </Label>
              }

            </div>

            <div
              className={styles.overview}
              style={{
                maxHeight: `${height}px`
              }}
            >
              <TextTruncate
                truncateText="…"
                line={Math.floor(height / (defaultFontSize * lineHeight))}
                text={stripHtml(overview)}
              />
            </div>
          </div>
        </div>

        <AddNewBookModal
          isOpen={isNewAddBookModalOpen && !isExistingBook}
          isExistingAuthor={isExistingAuthor}
          foreignBookId={foreignBookId}
          bookTitle={title}
          disambiguation={disambiguation}
          authorName={author.authorName}
          overview={overview}
          images={images}
          onModalClose={this.onAddBookModalClose}
        />
      </div>
    );
  }
}

AddNewBookSearchResult.propTypes = {
  foreignBookId: PropTypes.string.isRequired,
  titleSlug: PropTypes.string.isRequired,
  title: PropTypes.string.isRequired,
  releaseDate: PropTypes.string,
  disambiguation: PropTypes.string,
  overview: PropTypes.string,
  ratings: PropTypes.object.isRequired,
  author: PropTypes.object,
  editions: PropTypes.arrayOf(PropTypes.object).isRequired,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  isExistingBook: PropTypes.bool.isRequired,
  isExistingAuthor: PropTypes.bool.isRequired,
  isSmallScreen: PropTypes.bool.isRequired
};

export default AddNewBookSearchResult;
