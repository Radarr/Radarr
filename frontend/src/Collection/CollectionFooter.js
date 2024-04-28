import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import AvailabilitySelectInput from 'Components/Form/AvailabilitySelectInput';
import QualityProfileSelectInputConnector from 'Components/Form/QualityProfileSelectInputConnector';
import RootFolderSelectInputConnector from 'Components/Form/RootFolderSelectInputConnector';
import SelectInput from 'Components/Form/SelectInput';
import SpinnerButton from 'Components/Link/SpinnerButton';
import PageContentFooter from 'Components/Page/PageContentFooter';
import { kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import CollectionFooterLabel from './CollectionFooterLabel';
import styles from './CollectionFooter.css';

const NO_CHANGE = 'noChange';

const monitoredOptions = [
  {
    key: NO_CHANGE,
    get value() {
      return translate('NoChange');
    },
    isDisabled: true
  },
  {
    key: 'monitored',
    get value() {
      return translate('Monitored');
    }
  },
  {
    key: 'unmonitored',
    get value() {
      return translate('Unmonitored');
    }
  }
];

const searchOnAddOptions = [
  {
    key: NO_CHANGE,
    get value() {
      return translate('NoChange');
    },
    isDisabled: true
  },
  {
    key: 'yes',
    get value() {
      return translate('Yes');
    }
  },
  {
    key: 'no',
    get value() {
      return translate('No');
    }
  }
];

class CollectionFooter extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      monitored: NO_CHANGE,
      monitor: NO_CHANGE,
      qualityProfileId: NO_CHANGE,
      minimumAvailability: NO_CHANGE,
      rootFolderPath: NO_CHANGE,
      searchOnAdd: NO_CHANGE
    };
  }

  componentDidUpdate(prevProps) {
    const {
      isSaving,
      saveError
    } = this.props;

    const newState = {};
    if (prevProps.isSaving && !isSaving && !saveError) {
      this.setState({
        monitored: NO_CHANGE,
        monitor: NO_CHANGE,
        qualityProfileId: NO_CHANGE,
        minimumAvailability: NO_CHANGE,
        rootFolderPath: NO_CHANGE,
        searchOnAdd: NO_CHANGE
      });
    }

    if (!_.isEmpty(newState)) {
      this.setState(newState);
    }
  }

  //
  // Listeners

  onInputChange = ({ name, value }) => {
    this.setState({ [name]: value });
  };

  onUpdateSelectedPress = () => {
    const {
      monitored,
      monitor,
      qualityProfileId,
      minimumAvailability,
      rootFolderPath,
      searchOnAdd
    } = this.state;

    const changes = {};

    if (monitored !== NO_CHANGE) {
      changes.monitored = monitored === 'monitored';
    }

    if (monitor !== NO_CHANGE) {
      changes.monitor = monitor;
    }

    if (qualityProfileId !== NO_CHANGE) {
      changes.qualityProfileId = qualityProfileId;
    }

    if (minimumAvailability !== NO_CHANGE) {
      changes.minimumAvailability = minimumAvailability;
    }

    if (rootFolderPath !== NO_CHANGE) {
      changes.rootFolderPath = rootFolderPath;
    }

    if (searchOnAdd !== NO_CHANGE) {
      changes.searchOnAdd = searchOnAdd === 'yes';
    }

    this.props.onUpdateSelectedPress(changes);
  };

  //
  // Render

  render() {
    const {
      selectedIds,
      isSaving
    } = this.props;

    const {
      monitored,
      monitor,
      qualityProfileId,
      minimumAvailability,
      rootFolderPath,
      searchOnAdd
    } = this.state;

    const selectedCount = selectedIds.length;

    return (
      <PageContentFooter>
        <div className={styles.inputContainer}>
          <CollectionFooterLabel
            label={translate('MonitorCollection')}
            isSaving={isSaving && monitored !== NO_CHANGE}
          />

          <SelectInput
            name="monitored"
            value={monitored}
            values={monitoredOptions}
            isDisabled={!selectedCount}
            onChange={this.onInputChange}
          />
        </div>

        <div className={styles.inputContainer}>
          <CollectionFooterLabel
            label={translate('MonitorMovies')}
            isSaving={isSaving && monitor !== NO_CHANGE}
          />

          <SelectInput
            name="monitor"
            value={monitor}
            values={monitoredOptions}
            isDisabled={!selectedCount}
            onChange={this.onInputChange}
          />
        </div>

        <div className={styles.inputContainer}>
          <CollectionFooterLabel
            label={translate('QualityProfile')}
            isSaving={isSaving && qualityProfileId !== NO_CHANGE}
          />

          <QualityProfileSelectInputConnector
            name="qualityProfileId"
            value={qualityProfileId}
            includeNoChange={true}
            isDisabled={!selectedCount}
            onChange={this.onInputChange}
          />
        </div>

        <div className={styles.inputContainer}>
          <CollectionFooterLabel
            label={translate('MinimumAvailability')}
            isSaving={isSaving && minimumAvailability !== NO_CHANGE}
          />

          <AvailabilitySelectInput
            name="minimumAvailability"
            value={minimumAvailability}
            includeNoChange={true}
            isDisabled={!selectedCount}
            onChange={this.onInputChange}
          />
        </div>

        <div className={styles.inputContainer}>
          <CollectionFooterLabel
            label={translate('RootFolder')}
            isSaving={isSaving && rootFolderPath !== NO_CHANGE}
          />

          <RootFolderSelectInputConnector
            name="rootFolderPath"
            value={rootFolderPath}
            includeNoChange={true}
            isDisabled={!selectedCount}
            selectedValueOptions={{ includeFreeSpace: false }}
            onChange={this.onInputChange}
          />
        </div>

        <div className={styles.inputContainer}>
          <CollectionFooterLabel
            label={translate('SearchMoviesOnAdd')}
            isSaving={isSaving && searchOnAdd !== NO_CHANGE}
          />

          <SelectInput
            name="searchOnAdd"
            value={searchOnAdd}
            values={searchOnAddOptions}
            isDisabled={!selectedCount}
            onChange={this.onInputChange}
          />
        </div>

        <div className={styles.buttonContainer}>
          <div className={styles.buttonContainerContent}>
            <CollectionFooterLabel
              label={translate('CountCollectionsSelected', { count: selectedCount })}
              isSaving={false}
            />

            <div className={styles.buttons}>
              <div>
                <SpinnerButton
                  className={styles.addSelectedButton}
                  kind={kinds.PRIMARY}
                  isSpinning={isSaving}
                  isDisabled={!selectedCount || isSaving}
                  onPress={this.onUpdateSelectedPress}
                >
                  {translate('UpdateSelected')}
                </SpinnerButton>
              </div>
            </div>
          </div>
        </div>
      </PageContentFooter>
    );
  }
}

CollectionFooter.propTypes = {
  selectedIds: PropTypes.arrayOf(PropTypes.number).isRequired,
  isAdding: PropTypes.bool.isRequired,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  onUpdateSelectedPress: PropTypes.func.isRequired
};

export default CollectionFooter;
