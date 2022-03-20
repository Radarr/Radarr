import PropTypes from 'prop-types';
import React, { Component } from 'react';
import TextTruncate from 'react-text-truncate';
import CheckInput from 'Components/Form/CheckInput';
import Icon from 'Components/Icon';
import ImportListListConnector from 'Components/ImportListListConnector';
import Label from 'Components/Label';
import IconButton from 'Components/Link/IconButton';
import Link from 'Components/Link/Link';
import Popover from 'Components/Tooltip/Popover';
import AddNewDiscoverMovieModal from 'DiscoverMovie/AddNewDiscoverMovieModal';
import ExcludeMovieModal from 'DiscoverMovie/Exclusion/ExcludeMovieModal';
import { icons, kinds } from 'Helpers/Props';
import MovieDetailsLinks from 'Movie/Details/MovieDetailsLinks';
import MoviePoster from 'Movie/MoviePoster';
import dimensions from 'Styles/Variables/dimensions';
import fonts from 'Styles/Variables/fonts';
import translate from 'Utilities/String/translate';
import DiscoverMovieOverviewInfo from './DiscoverMovieOverviewInfo';
import styles from './DiscoverMovieOverview.css';

const columnPadding = parseInt(dimensions.movieIndexColumnPadding);
const columnPaddingSmallScreen = parseInt(dimensions.movieIndexColumnPaddingSmallScreen);
const defaultFontSize = parseInt(fonts.defaultFontSize);
const lineHeight = parseFloat(fonts.lineHeight);

// Hardcoded height beased on line-height of 32 + bottom margin of 10. 19 + 5 for List Row
// Less side-effecty than using react-measure.
const titleRowHeight = 66;

function getContentHeight(rowHeight, isSmallScreen) {
  const padding = isSmallScreen ? columnPaddingSmallScreen : columnPadding;

  return rowHeight - padding;
}

class DiscoverMovieOverview extends Component {

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
  };

  onAddMovieModalClose = () => {
    this.setState({ isNewAddMovieModalOpen: false });
  };

  onExcludeMoviePress = () => {
    this.setState({ isExcludeMovieModalOpen: true });
  };

  onExcludeMovieModalClose = () => {
    this.setState({ isExcludeMovieModalOpen: false });
  };

  onChange = ({ value, shiftKey }) => {
    const {
      tmdbId,
      onSelectedChange
    } = this.props;

    onSelectedChange({ id: tmdbId, value, shiftKey });
  };

  //
  // Render

  render() {
    const {
      tmdbId,
      imdbId,
      youTubeTrailerId,
      title,
      folder,
      year,
      overview,
      images,
      lists,
      posterWidth,
      posterHeight,
      rowHeight,
      isSmallScreen,
      isExisting,
      isExcluded,
      isRecommendation,
      isSelected,
      overviewOptions,
      ...otherProps
    } = this.props;

    const {
      isNewAddMovieModalOpen,
      isExcludeMovieModalOpen
    } = this.state;

    const elementStyle = {
      width: `${posterWidth}px`,
      height: `${posterHeight}px`
    };

    const linkProps = isExisting ? { to: `/movie/${tmdbId}` } : { onPress: this.onPress };

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
                {title}

                {
                  isExisting ?
                    <Icon
                      className={styles.alreadyExistsIcon}
                      name={icons.CHECK_CIRCLE}
                      size={30}
                      title={translate('AlreadyInYourLibrary')}
                    /> : null
                }
                {
                  isExcluded &&
                    <Icon
                      className={styles.exclusionIcon}
                      name={icons.DANGER}
                      size={30}
                      title={translate('MovieAlreadyExcluded')}
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
                    title={translate('Links')}
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
                  title={isExcluded ? translate('MovieAlreadyExcluded') : translate('ExcludeMovie')}
                  onPress={this.onExcludeMoviePress}
                  isDisabled={isExcluded}
                />
              </div>
            </div>

            <div className={styles.lists}>
              {
                isRecommendation ?
                  <Label
                    kind={kinds.INFO}
                  >
                    <Icon
                      name={icons.RECOMMENDED}
                      size={10}
                      title={translate('MovieIsRecommend')}
                    />
                  </Label> :
                  null
              }

              <ImportListListConnector
                lists={lists}
              />
            </div>

            <div className={styles.details}>
              <div className={styles.overview}>
                <TextTruncate
                  line={Math.floor(overviewHeight / (defaultFontSize * lineHeight))}
                  text={overview}
                />
              </div>

              <DiscoverMovieOverviewInfo
                height={overviewHeight}
                year={year}
                {...overviewOptions}
                {...otherProps}
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

DiscoverMovieOverview.propTypes = {
  tmdbId: PropTypes.number.isRequired,
  imdbId: PropTypes.string,
  youTubeTrailerId: PropTypes.string,
  title: PropTypes.string.isRequired,
  folder: PropTypes.string.isRequired,
  year: PropTypes.number.isRequired,
  overview: PropTypes.string.isRequired,
  status: PropTypes.string.isRequired,
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
  isRecommendation: PropTypes.bool.isRequired,
  isSelected: PropTypes.bool,
  lists: PropTypes.arrayOf(PropTypes.number).isRequired,
  onSelectedChange: PropTypes.func.isRequired
};

DiscoverMovieOverview.defaultProps = {
  lists: []
};

export default DiscoverMovieOverview;
