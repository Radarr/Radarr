import PropTypes from 'prop-types';
import React, { Component } from 'react';
import TextTruncate from 'react-text-truncate';
import dimensions from 'Styles/Variables/dimensions';
import fonts from 'Styles/Variables/fonts';
import { icons, kinds, sizes } from 'Helpers/Props';
import HeartRating from 'Components/HeartRating';
import Icon from 'Components/Icon';
import Label from 'Components/Label';
import Link from 'Components/Link/Link';
import AuthorPoster from 'Author/AuthorPoster';
import AddNewAuthorModal from './AddNewAuthorModal';
import styles from './AddNewAuthorSearchResult.css';

const columnPadding = parseInt(dimensions.authorIndexColumnPadding);
const columnPaddingSmallScreen = parseInt(dimensions.authorIndexColumnPaddingSmallScreen);
const defaultFontSize = parseInt(fonts.defaultFontSize);
const lineHeight = parseFloat(fonts.lineHeight);

function calculateHeight(rowHeight, isSmallScreen) {
  let height = rowHeight - 45;

  if (isSmallScreen) {
    height -= columnPaddingSmallScreen;
  } else {
    height -= columnPadding;
  }

  return height;
}

class AddNewAuthorSearchResult extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isNewAddAuthorModalOpen: false
    };
  }

  componentDidUpdate(prevProps) {
    if (!prevProps.isExistingAuthor && this.props.isExistingAuthor) {
      this.onAddAuthorModalClose();
    }
  }

  //
  // Listeners

  onPress = () => {
    this.setState({ isNewAddAuthorModalOpen: true });
  }

  onAddAuthorModalClose = () => {
    this.setState({ isNewAddAuthorModalOpen: false });
  }

  onMBLinkPress = (event) => {
    event.stopPropagation();
  }

  //
  // Render

  render() {
    const {
      foreignAuthorId,
      goodreadsId,
      titleSlug,
      authorName,
      year,
      disambiguation,
      authorType,
      status,
      overview,
      ratings,
      images,
      isExistingAuthor,
      isSmallScreen
    } = this.props;

    const {
      isNewAddAuthorModalOpen
    } = this.state;

    const linkProps = isExistingAuthor ? { to: `/author/${titleSlug}` } : { onPress: this.onPress };

    const endedString = authorType === 'Person' ? 'Deceased' : 'Ended';

    const height = calculateHeight(230, isSmallScreen);

    return (
      <div className={styles.searchResult}>
        <Link
          className={styles.underlay}
          {...linkProps}
        />

        <div className={styles.overlay}>
          {
            isSmallScreen ?
              null :
              <AuthorPoster
                className={styles.poster}
                images={images}
                size={250}
                overflow={true}
                lazy={false}
              />
          }

          <div className={styles.content}>
            <div className={styles.name}>
              {authorName}

              {
                !name.contains(year) && year ?
                  <span className={styles.year}>
                    ({year})
                  </span> :
                  null
              }

              {
                !!disambiguation &&
                <span className={styles.year}>({disambiguation})</span>
              }

              {
                isExistingAuthor ?
                  <Icon
                    className={styles.alreadyExistsIcon}
                    name={icons.CHECK_CIRCLE}
                    size={36}
                    title="Already in your library"
                  /> :
                  null
              }

              <Link
                className={styles.mbLink}
                to={`https://goodreads.com/author/show/${goodreadsId}`}
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
              <Label size={sizes.LARGE}>
                <HeartRating
                  rating={ratings.value}
                  iconSize={13}
                />
              </Label>

              {
                authorType ?
                  <Label size={sizes.LARGE}>
                    {authorType}
                  </Label> :
                  null
              }

              {
                status === 'ended' ?
                  <Label
                    kind={kinds.DANGER}
                    size={sizes.LARGE}
                  >
                    {endedString}
                  </Label> :
                  null
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
                text={overview}
              />
            </div>
          </div>
        </div>

        <AddNewAuthorModal
          isOpen={isNewAddAuthorModalOpen && !isExistingAuthor}
          foreignAuthorId={foreignAuthorId}
          authorName={authorName}
          disambiguation={disambiguation}
          year={year}
          overview={overview}
          images={images}
          onModalClose={this.onAddAuthorModalClose}
        />
      </div>
    );
  }
}

AddNewAuthorSearchResult.propTypes = {
  foreignAuthorId: PropTypes.string.isRequired,
  goodreadsId: PropTypes.number.isRequired,
  titleSlug: PropTypes.string.isRequired,
  authorName: PropTypes.string.isRequired,
  year: PropTypes.number,
  disambiguation: PropTypes.string,
  authorType: PropTypes.string,
  status: PropTypes.string.isRequired,
  overview: PropTypes.string,
  ratings: PropTypes.object.isRequired,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  isExistingAuthor: PropTypes.bool.isRequired,
  isSmallScreen: PropTypes.bool.isRequired
};

export default AddNewAuthorSearchResult;
