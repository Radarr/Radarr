import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import AvailabilitySelectInput from 'Components/Form/AvailabilitySelectInput';
import CheckInput from 'Components/Form/CheckInput';
import QualityProfileSelectInputConnector from 'Components/Form/QualityProfileSelectInputConnector';
import RootFolderSelectInputConnector from 'Components/Form/RootFolderSelectInputConnector';
import SelectInput from 'Components/Form/SelectInput';
import SpinnerButton from 'Components/Link/SpinnerButton';
import PageContentFooter from 'Components/Page/PageContentFooter';
import { kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import DiscoverMovieFooterLabel from './DiscoverMovieFooterLabel';
import ExcludeMovieModal from './Exclusion/ExcludeMovieModal';
import styles from './DiscoverMovieFooter.css';

class DiscoverMovieFooter extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    const {
      defaultMonitor,
      defaultQualityProfileId,
      defaultMinimumAvailability,
      defaultRootFolderPath,
      defaultSearchForMovie
    } = props;

    this.state = {
      monitor: defaultMonitor,
      qualityProfileId: defaultQualityProfileId,
      minimumAvailability: defaultMinimumAvailability,
      rootFolderPath: defaultRootFolderPath,
      searchForMovie: defaultSearchForMovie,
      isExcludeMovieModalOpen: false,
      destinationRootFolder: null
    };
  }

  componentDidUpdate(prevProps) {
    const {
      defaultMonitor,
      defaultQualityProfileId,
      defaultMinimumAvailability,
      defaultRootFolderPath,
      defaultSearchForMovie
    } = this.props;

    const {
      monitor,
      qualityProfileId,
      minimumAvailability,
      rootFolderPath,
      searchForMovie
    } = this.state;

    const newState = {};

    if (monitor !== defaultMonitor) {
      newState.monitor = defaultMonitor;
    }

    if (qualityProfileId !== defaultQualityProfileId) {
      newState.qualityProfileId = defaultQualityProfileId;
    }

    if (minimumAvailability !== defaultMinimumAvailability) {
      newState.minimumAvailability = defaultMinimumAvailability;
    }

    if (rootFolderPath !== defaultRootFolderPath) {
      newState.rootFolderPath = defaultRootFolderPath;
    }

    if (searchForMovie !== defaultSearchForMovie) {
      newState.searchForMovie = defaultSearchForMovie;
    }

    if (!_.isEmpty(newState)) {
      this.setState(newState);
    }
  }

  //
  // Listeners

  onExcludeSelectedPress = () => {
    this.setState({ isExcludeMovieModalOpen: true });
  };

  onExcludeMovieModalClose = () => {
    this.setState({ isExcludeMovieModalOpen: false });
  };

  onAddMoviesPress = () => {
    const {
      monitor,
      qualityProfileId,
      minimumAvailability,
      rootFolderPath,
      searchForMovie
    } = this.state;

    const addOptions = {
      monitor,
      qualityProfileId,
      minimumAvailability,
      rootFolderPath,
      searchForMovie
    };

    this.props.onAddMoviesPress({ addOptions });
  };

  //
  // Render

  render() {
    const {
      selectedIds,
      selectedCount,
      isAdding,
      isExcluding,
      onInputChange
    } = this.props;

    const {
      monitor,
      qualityProfileId,
      minimumAvailability,
      rootFolderPath,
      searchForMovie,
      isExcludeMovieModalOpen
    } = this.state;

    const monitoredOptions = [
      { key: true, value: translate('Monitored') },
      { key: false, value: translate('Unmonitored') }
    ];

    return (
      <PageContentFooter>
        <div className={styles.inputContainer}>
          <DiscoverMovieFooterLabel
            label={translate('MonitorMovie')}
            isSaving={isAdding}
          />

          <SelectInput
            name="monitor"
            value={monitor}
            values={monitoredOptions}
            isDisabled={!selectedCount}
            onChange={onInputChange}
          />
        </div>

        <div className={styles.inputContainer}>
          <DiscoverMovieFooterLabel
            label={translate('QualityProfile')}
            isSaving={isAdding}
          />

          <QualityProfileSelectInputConnector
            name="qualityProfileId"
            value={qualityProfileId}
            isDisabled={!selectedCount}
            onChange={onInputChange}
          />
        </div>

        <div className={styles.inputContainer}>
          <DiscoverMovieFooterLabel
            label={translate('MinimumAvailability')}
            isSaving={isAdding}
          />

          <AvailabilitySelectInput
            name="minimumAvailability"
            value={minimumAvailability}
            isDisabled={!selectedCount}
            onChange={onInputChange}
          />
        </div>

        <div className={styles.inputContainer}>
          <DiscoverMovieFooterLabel
            label={translate('RootFolder')}
            isSaving={isAdding}
          />

          <RootFolderSelectInputConnector
            name="rootFolderPath"
            value={rootFolderPath}
            isDisabled={!selectedCount}
            selectedValueOptions={{ includeFreeSpace: false }}
            onChange={onInputChange}
          />
        </div>

        <div className={styles.inputContainer}>
          <DiscoverMovieFooterLabel
            label={translate('SearchOnAdd')}
            isSaving={isAdding}
          />

          <CheckInput
            name="searchForMovie"
            isDisabled={!selectedCount}
            value={searchForMovie}
            onChange={onInputChange}
          />
        </div>

        <div className={styles.buttonContainer}>
          <div className={styles.buttonContainerContent}>
            <DiscoverMovieFooterLabel
              label={translate('MoviesSelectedInterp', [selectedCount])}
              isSaving={false}
            />

            <div className={styles.buttons}>
              <div>
                <SpinnerButton
                  className={styles.addSelectedButton}
                  kind={kinds.PRIMARY}
                  isSpinning={isAdding}
                  isDisabled={!selectedCount || isAdding}
                  onPress={this.onAddMoviesPress}
                >
                  {translate('AddMovies')}
                </SpinnerButton>

                <SpinnerButton
                  className={styles.excludeSelectedButton}
                  kind={kinds.DANGER}
                  isSpinning={isExcluding}
                  isDisabled={!selectedCount || isExcluding}
                  onPress={this.props.onExcludeMoviesPress}
                >
                  {translate('AddExclusion')}
                </SpinnerButton>
              </div>
            </div>
          </div>
        </div>

        <ExcludeMovieModal
          isOpen={isExcludeMovieModalOpen}
          movieIds={selectedIds}
          onModalClose={this.onExcludeMovieModalClose}
        />
      </PageContentFooter>
    );
  }
}

DiscoverMovieFooter.propTypes = {
  selectedIds: PropTypes.arrayOf(PropTypes.number).isRequired,
  selectedCount: PropTypes.number.isRequired,
  isAdding: PropTypes.bool.isRequired,
  isExcluding: PropTypes.bool.isRequired,
  defaultMonitor: PropTypes.string.isRequired,
  defaultQualityProfileId: PropTypes.number,
  defaultMinimumAvailability: PropTypes.string,
  defaultRootFolderPath: PropTypes.string,
  defaultSearchForMovie: PropTypes.bool,
  onInputChange: PropTypes.func.isRequired,
  onAddMoviesPress: PropTypes.func.isRequired,
  onExcludeMoviesPress: PropTypes.func.isRequired
};

export default DiscoverMovieFooter;
