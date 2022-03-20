import PropTypes from 'prop-types';
import React, { Component } from 'react';
import CheckInput from 'Components/Form/CheckInput';
import Icon from 'Components/Icon';
import Label from 'Components/Label';
import IconButton from 'Components/Link/IconButton';
import Link from 'Components/Link/Link';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import Popover from 'Components/Tooltip/Popover';
import { icons } from 'Helpers/Props';
import DeleteMovieModal from 'Movie/Delete/DeleteMovieModal';
import MovieDetailsLinks from 'Movie/Details/MovieDetailsLinks';
import EditMovieModalConnector from 'Movie/Edit/EditMovieModalConnector';
import MovieIndexProgressBar from 'Movie/Index/ProgressBar/MovieIndexProgressBar';
import MoviePoster from 'Movie/MoviePoster';
import getRelativeDate from 'Utilities/Date/getRelativeDate';
import translate from 'Utilities/String/translate';
import MovieIndexPosterInfo from './MovieIndexPosterInfo';
import styles from './MovieIndexPoster.css';

class MovieIndexPoster extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      hasPosterError: false,
      isEditMovieModalOpen: false,
      isDeleteMovieModalOpen: false
    };
  }

  //
  // Listeners

  onEditMoviePress = () => {
    this.setState({ isEditMovieModalOpen: true });
  };

  onEditMovieModalClose = () => {
    this.setState({ isEditMovieModalOpen: false });
  };

  onDeleteMoviePress = () => {
    this.setState({
      isEditMovieModalOpen: false,
      isDeleteMovieModalOpen: true
    });
  };

  onDeleteMovieModalClose = () => {
    this.setState({ isDeleteMovieModalOpen: false });
  };

  onPosterLoad = () => {
    if (this.state.hasPosterError) {
      this.setState({ hasPosterError: false });
    }
  };

  onPosterLoadError = () => {
    if (!this.state.hasPosterError) {
      this.setState({ hasPosterError: true });
    }
  };

  onChange = ({ value, shiftKey }) => {
    const {
      id,
      onSelectedChange
    } = this.props;

    onSelectedChange({ id, value, shiftKey });
  };

  //
  // Render

  render() {
    const {
      id,
      tmdbId,
      imdbId,
      youTubeTrailerId,
      title,
      monitored,
      hasFile,
      isAvailable,
      status,
      titleSlug,
      images,
      posterWidth,
      posterHeight,
      detailedProgressBar,
      showTitle,
      showMonitored,
      showQualityProfile,
      qualityProfile,
      showSearchAction,
      showRelativeDates,
      shortDateFormat,
      showReleaseDate,
      showCinemaRelease,
      inCinemas,
      physicalRelease,
      digitalRelease,
      timeFormat,
      isRefreshingMovie,
      isSearchingMovie,
      onRefreshMoviePress,
      onSearchPress,
      isMovieEditorActive,
      isSelected,
      onSelectedChange,
      queueStatus,
      queueState,
      ...otherProps
    } = this.props;

    const {
      hasPosterError,
      isEditMovieModalOpen,
      isDeleteMovieModalOpen
    } = this.state;

    const link = `/movie/${titleSlug}`;

    const elementStyle = {
      width: `${posterWidth}px`,
      height: `${posterHeight}px`
    };

    let releaseDate = '';
    let releaseDateType = '';
    if (physicalRelease && digitalRelease) {
      releaseDate = (physicalRelease < digitalRelease) ? physicalRelease : digitalRelease;
      releaseDateType = (physicalRelease < digitalRelease) ? 'Released' : 'Digital';
    } else if (physicalRelease && !digitalRelease) {
      releaseDate = physicalRelease;
      releaseDateType = 'Released';
    } else if (digitalRelease && !physicalRelease) {
      releaseDate = digitalRelease;
      releaseDateType = 'Digital';
    }

    return (
      <div className={styles.content}>
        <div className={styles.posterContainer}>
          {
            isMovieEditorActive &&
              <div className={styles.editorSelect}>
                <CheckInput
                  className={styles.checkInput}
                  name={id.toString()}
                  value={isSelected}
                  onChange={this.onChange}
                />
              </div>
          }
          <Label className={styles.controls}>
            <SpinnerIconButton
              className={styles.action}
              name={icons.REFRESH}
              title={translate('RefreshMovie')}
              isSpinning={isRefreshingMovie}
              onPress={onRefreshMoviePress}
            />

            {
              showSearchAction &&
                <SpinnerIconButton
                  className={styles.action}
                  name={icons.SEARCH}
                  title={translate('SearchForMovie')}
                  isSpinning={isSearchingMovie}
                  onPress={onSearchPress}
                />
            }

            <IconButton
              className={styles.action}
              name={icons.EDIT}
              title={translate('EditMovie')}
              onPress={this.onEditMoviePress}
            />

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
          </Label>

          {
            status === 'ended' &&
              <div
                className={styles.ended}
                title={translate('Ended')}
              />
          }

          <Link
            className={styles.link}
            style={elementStyle}
            to={link}
          >
            <MoviePoster
              className={styles.poster}
              style={elementStyle}
              images={images}
              size={250}
              lazy={false}
              overflow={true}
              onError={this.onPosterLoadError}
              onLoad={this.onPosterLoad}
            />

            {
              hasPosterError &&
                <div className={styles.overlayTitle}>
                  {title}
                </div>
            }
          </Link>
        </div>

        <MovieIndexProgressBar
          monitored={monitored}
          hasFile={hasFile}
          status={status}
          posterWidth={posterWidth}
          detailedProgressBar={detailedProgressBar}
          queueStatus={queueStatus}
          queueState={queueState}
          isAvailable={isAvailable}
        />

        {
          showTitle &&
            <div className={styles.title}>
              {title}
            </div>
        }

        {
          showMonitored &&
            <div className={styles.title}>
              {monitored ? translate('Monitored') : translate('Unmonitored')}
            </div>
        }

        {
          showQualityProfile &&
            <div className={styles.title}>
              {qualityProfile.name}
            </div>
        }

        {
          showCinemaRelease && inCinemas &&
            <div className={styles.title}>
              <Icon
                name={icons.IN_CINEMAS}
              /> {getRelativeDate(
                inCinemas,
                shortDateFormat,
                showRelativeDates,
                {
                  timeFormat,
                  timeForToday: false
                }
              )}
            </div>
        }

        {
          showReleaseDate && releaseDateType === 'Released' &&
            <div className={styles.title}>
              <Icon
                name={icons.DISC}
              /> {getRelativeDate(
                releaseDate,
                shortDateFormat,
                showRelativeDates,
                {
                  timeFormat,
                  timeForToday: false
                }
              )}
            </div>
        }

        {
          showReleaseDate && releaseDateType === 'Digital' &&
            <div className={styles.title}>
              <Icon
                name={icons.MOVIE_FILE}
              /> {getRelativeDate(
                releaseDate,
                shortDateFormat,
                showRelativeDates,
                {
                  timeFormat,
                  timeForToday: false
                }
              )}
            </div>
        }

        <MovieIndexPosterInfo
          qualityProfile={qualityProfile}
          showQualityProfile={showQualityProfile}
          showReleaseDate={showReleaseDate}
          showRelativeDates={showRelativeDates}
          shortDateFormat={shortDateFormat}
          timeFormat={timeFormat}
          inCinemas={inCinemas}
          physicalRelease={physicalRelease}
          digitalRelease={digitalRelease}
          {...otherProps}
        />

        <EditMovieModalConnector
          isOpen={isEditMovieModalOpen}
          movieId={id}
          onModalClose={this.onEditMovieModalClose}
          onDeleteMoviePress={this.onDeleteMoviePress}
        />

        <DeleteMovieModal
          isOpen={isDeleteMovieModalOpen}
          movieId={id}
          onModalClose={this.onDeleteMovieModalClose}
        />
      </div>
    );
  }
}

MovieIndexPoster.propTypes = {
  id: PropTypes.number.isRequired,
  title: PropTypes.string.isRequired,
  monitored: PropTypes.bool.isRequired,
  hasFile: PropTypes.bool.isRequired,
  isAvailable: PropTypes.bool.isRequired,
  status: PropTypes.string.isRequired,
  titleSlug: PropTypes.string.isRequired,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  posterWidth: PropTypes.number.isRequired,
  posterHeight: PropTypes.number.isRequired,
  detailedProgressBar: PropTypes.bool.isRequired,
  showTitle: PropTypes.bool.isRequired,
  showMonitored: PropTypes.bool.isRequired,
  showQualityProfile: PropTypes.bool.isRequired,
  qualityProfile: PropTypes.object.isRequired,
  showSearchAction: PropTypes.bool.isRequired,
  showRelativeDates: PropTypes.bool.isRequired,
  shortDateFormat: PropTypes.string.isRequired,
  showCinemaRelease: PropTypes.bool.isRequired,
  showReleaseDate: PropTypes.bool.isRequired,
  inCinemas: PropTypes.string,
  physicalRelease: PropTypes.string,
  digitalRelease: PropTypes.string,
  timeFormat: PropTypes.string.isRequired,
  isRefreshingMovie: PropTypes.bool.isRequired,
  isSearchingMovie: PropTypes.bool.isRequired,
  onRefreshMoviePress: PropTypes.func.isRequired,
  onSearchPress: PropTypes.func.isRequired,
  isMovieEditorActive: PropTypes.bool.isRequired,
  isSelected: PropTypes.bool,
  onSelectedChange: PropTypes.func.isRequired,
  tmdbId: PropTypes.number.isRequired,
  imdbId: PropTypes.string,
  youTubeTrailerId: PropTypes.string,
  queueStatus: PropTypes.string,
  queueState: PropTypes.string
};

MovieIndexPoster.defaultProps = {
  statistics: {
    movieFileCount: 0
  }
};

export default MovieIndexPoster;
