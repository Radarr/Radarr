import PropTypes from 'prop-types';
import React, { Component } from 'react';
import TextTruncate from 'react-text-truncate';
import CheckInput from 'Components/Form/CheckInput';
import Icon from 'Components/Icon';
import IconButton from 'Components/Link/IconButton';
import Link from 'Components/Link/Link';
import Popover from 'Components/Tooltip/Popover';
import AddNewDiscoverMovieModal from 'DiscoverMovie/AddNewDiscoverMovieModal';
import ExcludeMovieModal from 'DiscoverMovie/Exclusion/ExcludeMovieModal';
import { icons } from 'Helpers/Props';
import MovieDetailsLinks from 'Movie/Details/MovieDetailsLinks';
import MoviePoster from 'Movie/MoviePoster';
import dimensions from 'Styles/Variables/dimensions';
import fonts from 'Styles/Variables/fonts';
import styles from './AddListMovieOverview.css';

const columnPadding = parseInt(dimensions.movieIndexColumnPadding);
const columnPaddingSmallScreen = parseInt(dimensions.movieIndexColumnPaddingSmallScreen);
const defaultFontSize = parseInt(fonts.defaultFontSize);
const lineHeight = parseFloat(fonts.lineHeight);

// Hardcoded height beased on line-height of 32 + bottom margin of 10.
// Less side-effecty than using react-measure.
const titleRowHeight = 42;

function getContentHeight(rowHeight, isSmallScreen) {
  const padding = isSmallScreen ? columnPaddingSmallScreen : columnPadding;

  return rowHeight - padding;
}

class AddListMovieOverview extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isNewAddMovieModalOpen: false,
      isExcludeMovieModalOpen: false
    };
  }

  //
  // Listeners

  onPress = () => {
    this.setState({ isNewAddMovieModalOpen: true });
  }

  onAddMovieModalClose = () => {
    this.setState({ isNewAddMovieModalOpen: false });
  }

  onExcludeMoviePress = () => {
    this.setState({ isExcludeMovieModalOpen: true });
  }

  onExcludeMovieModalClose = () => {
    this.setState({ isExcludeMovieModalOpen: false });
  }

  onChange = ({ value, shiftKey }) => {
    const {
      tmdbId,
      onSelectedChange
    } = this.props;

    onSelectedChange({ id: tmdbId, value, shiftKey });
  }

  //
  // Render

  render() {
    const {
      tmdbId,
      imdbId,
      youTubeTrailerId,
      title,
      titleSlug,
      folder,
      year,
      overview,
      images,
      posterWidth,
      posterHeight,
      rowHeight,
      isSmallScreen,
      isExisting,
      isExcluded,
      isSelected
    } = this.props;

    const {
      isNewAddMovieModalOpen,
      isExcludeMovieModalOpen
    } = this.state;

    const elementStyle = {
      width: `${posterWidth}px`,
      height: `${posterHeight}px`
    };

    const linkProps = isExisting ? { to: `/movie/${titleSlug}` } : { onPress: this.onPress };

    const contentHeight = getContentHeight(rowHeight, isSmallScreen);
    const overviewHeight = contentHeight - titleRowHeight;

    return (
      <div className={styles.container}>
        <div className={styles.content}>
          <div className={styles.poster}>
            <div className={styles.posterContainer}>
              <div className={styles.editorSelect}>
                <CheckInput
                  className={styles.checkInput}
                  name={tmdbId.toString()}
                  value={isSelected}
                  onChange={this.onChange}
                />
              </div>

              <MoviePoster
                className={styles.poster}
                style={elementStyle}
                images={images}
                size={250}
                lazy={false}
                overflow={true}
              />
            </div>
          </div>

          <div className={styles.info} style={{ maxHeight: contentHeight }}>
            <div className={styles.titleRow}>
              <Link
                className={styles.title}
                {...linkProps}
              >
                {title} { year > 0 ? `(${year})` : ''}

                {
                  isExcluded &&
                    <Icon
                      className={styles.exclusionIcon}
                      name={icons.DANGER}
                      size={36}
                      title='Movie is on Net Import Exclusion List'
                    />
                }
              </Link>

              <div className={styles.actions}>
                <span className={styles.externalLinks}>
                  <Popover
                    anchor={
                      <Icon
                        name={icons.EXTERNAL_LINK}
                        size={12}
                      />
                    }
                    title="Links"
                    body={
                      <MovieDetailsLinks
                        tmdbId={tmdbId}
                        imdbId={imdbId}
                        youTubeTrailerId={youTubeTrailerId}
                      />
                    }
                  />
                </span>

                <IconButton
                  name={icons.REMOVE}
                  title={isExcluded ? 'Movie already Excluded' : 'Exclude Movie'}
                  onPress={this.onExcludeMoviePress}
                  isDisabled={isExcluded}
                />
              </div>
            </div>

            <div className={styles.details}>
              <TextTruncate
                line={Math.floor(overviewHeight / (defaultFontSize * lineHeight))}
                text={overview}
              />

            </div>
          </div>
        </div>

        <AddNewDiscoverMovieModal
          isOpen={isNewAddMovieModalOpen && !isExisting}
          tmdbId={tmdbId}
          title={title}
          year={year}
          overview={overview}
          folder={folder}
          images={images}
          onModalClose={this.onAddMovieModalClose}
        />

        <ExcludeMovieModal
          isOpen={isExcludeMovieModalOpen}
          tmdbId={tmdbId}
          title={title}
          year={year}
          onModalClose={this.onExcludeMovieModalClose}
        />
      </div>
    );
  }
}

AddListMovieOverview.propTypes = {
  tmdbId: PropTypes.number.isRequired,
  imdbId: PropTypes.string,
  youTubeTrailerId: PropTypes.string,
  title: PropTypes.string.isRequired,
  folder: PropTypes.string.isRequired,
  year: PropTypes.number.isRequired,
  overview: PropTypes.string.isRequired,
  status: PropTypes.string.isRequired,
  titleSlug: PropTypes.string.isRequired,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  posterWidth: PropTypes.number.isRequired,
  posterHeight: PropTypes.number.isRequired,
  rowHeight: PropTypes.number.isRequired,
  overviewOptions: PropTypes.object.isRequired,
  showRelativeDates: PropTypes.bool.isRequired,
  shortDateFormat: PropTypes.string.isRequired,
  longDateFormat: PropTypes.string.isRequired,
  timeFormat: PropTypes.string.isRequired,
  isSmallScreen: PropTypes.bool.isRequired,
  isExisting: PropTypes.bool.isRequired,
  isExcluded: PropTypes.bool.isRequired,
  isSelected: PropTypes.bool,
  onSelectedChange: PropTypes.func.isRequired
};

export default AddListMovieOverview;
