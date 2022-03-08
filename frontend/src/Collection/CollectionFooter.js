import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import SelectInput from 'Components/Form/SelectInput';
import SpinnerButton from 'Components/Link/SpinnerButton';
import PageContentFooter from 'Components/Page/PageContentFooter';
import { kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import CollectionFooterLabel from './CollectionFooterLabel';
import styles from './CollectionFooter.css';

const NO_CHANGE = 'noChange';

class CollectionFooter extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      monitor: NO_CHANGE,
      monitored: NO_CHANGE,
      destinationRootFolder: null
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
        monitor: NO_CHANGE
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
      monitor,
      monitored
    } = this.state;

    const changes = {};

    if (monitored !== NO_CHANGE) {
      changes.monitored = monitored === 'monitored';
    }

    if (monitor !== NO_CHANGE) {
      changes.monitor = monitor;
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
      monitor
    } = this.state;

    const monitoredOptions = [
      { key: NO_CHANGE, value: translate('NoChange'), disabled: true },
      { key: 'monitored', value: translate('Monitored') },
      { key: 'unmonitored', value: translate('Unmonitored') }
    ];

    const selectedCount = selectedIds.length;

    return (
      <PageContentFooter>
        <div className={styles.inputContainer}>
          <CollectionFooterLabel
            label={translate('MonitorCollection')}
            isSaving={isSaving}
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
            isSaving={isSaving}
          />

          <SelectInput
            name="monitor"
            value={monitor}
            values={monitoredOptions}
            isDisabled={!selectedCount}
            onChange={this.onInputChange}
          />
        </div>

        <div className={styles.buttonContainer}>
          <div className={styles.buttonContainerContent}>
            <CollectionFooterLabel
              label={translate('CollectionsSelectedInterp', [selectedCount])}
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
