import classNames from 'classnames';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Icon from 'Components/Icon';
import Label from 'Components/Label';
import Link from 'Components/Link/Link';
import MonitorToggleButton from 'Components/MonitorToggleButton';
import Popover from 'Components/Tooltip/Popover';
import { icons, kinds, sizes } from 'Helpers/Props';
import MovieHeadshot from 'Movie/MovieHeadshot';
import EditImportListModalConnector from 'Settings/ImportLists/ImportLists/EditImportListModalConnector';
import translate from 'Utilities/String/translate';
import styles from '../MovieCreditPoster.css';

class MovieCrewPoster extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      hasPosterError: false,
      isEditImportListModalOpen: false
    };
  }

  //
  // Listeners

  onEditImportListPress = () => {
    this.setState({ isEditImportListModalOpen: true });
  };

  onAddImportListPress = () => {
    this.props.onImportListSelect();
    this.setState({ isEditImportListModalOpen: true });
  };

  onEditImportListModalClose = () => {
    this.setState({ isEditImportListModalOpen: false });
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

  //
  // Render

  render() {
    const {
      tmdbId,
      personName,
      job,
      images,
      posterWidth,
      posterHeight,
      importList
    } = this.props;

    const {
      hasPosterError
    } = this.state;

    const elementStyle = {
      width: `${posterWidth}px`,
      height: `${posterHeight}px`,
      borderRadius: '5px'
    };

    const contentStyle = {
      width: `${posterWidth}px`
    };

    const monitored = importList !== undefined && importList.enabled && importList.enableAuto;
    const importListId = importList ? importList.id : 0;

    return (
      <div
        className={styles.content}
        style={contentStyle}
      >
        <div className={styles.posterContainer}>
          <div className={styles.toggleMonitoredContainer}>
            <MonitorToggleButton
              className={styles.monitorToggleButton}
              monitored={monitored}
              size={20}
              onPress={importListId > 0 ? this.onEditImportListPress : this.onAddImportListPress}
            />
          </div>

          <Label className={styles.controls}>
            <span className={styles.externalLinks}>
              <Popover
                anchor={<Icon name={icons.EXTERNAL_LINK} size={12} />}
                title={translate('Links')}
                body={
                  <Link to={`https://www.themoviedb.org/person/${tmdbId}`}>
                    <Label
                      className={styles.externalLinkLabel}
                      kind={kinds.INFO}
                      size={sizes.LARGE}
                    >
                      {translate('TMDb')}
                    </Label>
                  </Link>
                }
              />
            </span>
          </Label>

          <div
            style={elementStyle}
          >
            <MovieHeadshot
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
                  {personName}
                </div>
            }
          </div>
        </div>

        <div className={classNames(styles.title, 'swiper-no-swiping')}>
          {personName}
        </div>
        <div className={classNames(styles.title, 'swiper-no-swiping')}>
          {job}
        </div>

        <EditImportListModalConnector
          id={importListId}
          isOpen={this.state.isEditImportListModalOpen}
          onModalClose={this.onEditImportListModalClose}
          onDeleteImportListPress={this.onDeleteImportListPress}
        />
      </div>
    );
  }
}

MovieCrewPoster.propTypes = {
  tmdbId: PropTypes.number.isRequired,
  personName: PropTypes.string.isRequired,
  job: PropTypes.string.isRequired,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  posterWidth: PropTypes.number.isRequired,
  posterHeight: PropTypes.number.isRequired,
  importList: PropTypes.object,
  onImportListSelect: PropTypes.func.isRequired
};

export default MovieCrewPoster;
