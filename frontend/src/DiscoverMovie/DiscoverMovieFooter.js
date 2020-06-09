import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { kinds } from 'Helpers/Props';
import SelectInput from 'Components/Form/SelectInput';
import AvailabilitySelectInput from 'Components/Form/AvailabilitySelectInput';
import QualityProfileSelectInputConnector from 'Components/Form/QualityProfileSelectInputConnector';
import RootFolderSelectInputConnector from 'Components/Form/RootFolderSelectInputConnector';
import SpinnerButton from 'Components/Link/SpinnerButton';
import PageContentFooter from 'Components/Page/PageContentFooter';
import ExcludeMovieModal from './Exclusion/ExcludeMovieModal';
import DiscoverMovieFooterLabel from './DiscoverMovieFooterLabel';
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
      defaultRootFolderPath
    } = props;

    this.state = {
      monitor: defaultMonitor,
      qualityProfileId: defaultQualityProfileId,
      minimumAvailability: defaultMinimumAvailability,
      rootFolderPath: defaultRootFolderPath,
      isExcludeMovieModalOpen: false,
      destinationRootFolder: null
    };
  }

  componentDidUpdate(prevProps) {
    const {
      defaultMonitor,
      defaultQualityProfileId,
      defaultMinimumAvailability,
      defaultRootFolderPath
    } = this.props;

    const {
      monitor,
      qualityProfileId,
      minimumAvailability,
      rootFolderPath
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

    if (!_.isEmpty(newState)) {
      this.setState(newState);
    }
  }

  //
  // Listeners

  //
  // Listeners

  onExcludeSelectedPress = () => {
    this.setState({ isExcludeMovieModalOpen: true });
  }

  onExcludeMovieModalClose = () => {
    this.setState({ isExcludeMovieModalOpen: false });
  }

  onAddMoviesPress = () => {
    const {
      monitor,
      qualityProfileId,
      minimumAvailability,
      rootFolderPath
    } = this.state;

    const addOptions = {
      monitor,
      qualityProfileId,
      minimumAvailability,
      rootFolderPath
    };

    this.props.onAddMoviesPress({ addOptions });
  }

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
      isExcludeMovieModalOpen
    } = this.state;

    const monitoredOptions = [
      { key: true, value: 'Monitored' },
      { key: false, value: 'Unmonitored' }
    ];

    return (
      <PageContentFooter>
        <div className={styles.inputContainer}>
          <DiscoverMovieFooterLabel
            label="Monitor Movie"
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
            label="Quality Profile"
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
            label="Minimum Availability"
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
            label="Root Folder"
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

        <div className={styles.buttonContainer}>
          <div className={styles.buttonContainerContent}>
            <DiscoverMovieFooterLabel
              label={`${selectedCount} Movie(s) Selected`}
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
                  Add Movies
                </SpinnerButton>
              </div>

              <SpinnerButton
                className={styles.excludeSelectedButton}
                kind={kinds.DANGER}
                isSpinning={isExcluding}
                isDisabled={!selectedCount || isExcluding}
                onPress={this.props.onExcludeMoviesPress}
              >
                Add Exclusion
              </SpinnerButton>
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
  onInputChange: PropTypes.func.isRequired,
  onAddMoviesPress: PropTypes.func.isRequired,
  onExcludeMoviesPress: PropTypes.func.isRequired
};

export default DiscoverMovieFooter;
