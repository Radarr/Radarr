import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { inputTypes, kinds } from 'Helpers/Props';
import Button from 'Components/Link/Button';
import SpinnerButton from 'Components/Link/SpinnerButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
// import CheckInput from 'Components/Form/CheckInput';
import FormInputGroup from 'Components/Form/FormInputGroup';
import PageContentFooter from 'Components/Page/PageContentFooter';
import styles from './ImportMovieFooter.css';

const MIXED = 'mixed';

class ImportMovieFooter extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    const {
      defaultMonitor,
      defaultQualityProfileId
    } = props;

    this.state = {
      monitor: defaultMonitor,
      qualityProfileId: defaultQualityProfileId
    };
  }

  componentDidUpdate(prevProps, prevState) {
    const {
      defaultMonitor,
      defaultQualityProfileId,
      isMonitorMixed,
      isQualityProfileIdMixed
    } = this.props;

    const {
      monitor,
      qualityProfileId
    } = this.state;

    const newState = {};

    if (isMonitorMixed && monitor !== MIXED) {
      newState.monitor = MIXED;
    } else if (!isMonitorMixed && monitor !== defaultMonitor) {
      newState.monitor = defaultMonitor;
    }

    if (isQualityProfileIdMixed && qualityProfileId !== MIXED) {
      newState.qualityProfileId = MIXED;
    } else if (!isQualityProfileIdMixed && qualityProfileId !== defaultQualityProfileId) {
      newState.qualityProfileId = defaultQualityProfileId;
    }

    if (!_.isEmpty(newState)) {
      this.setState(newState);
    }
  }

  //
  // Listeners

  onInputChange = ({ name, value }) => {
    this.setState({ [name]: value });
    this.props.onInputChange({ name, value });
  }

  //
  // Render

  render() {
    const {
      selectedCount,
      isImporting,
      isLookingUpMovie,
      isMonitorMixed,
      isQualityProfileIdMixed,
      onImportPress,
      onCancelLookupPress
    } = this.props;

    const {
      monitor,
      qualityProfileId
    } = this.state;

    return (
      <PageContentFooter>
        <div className={styles.inputContainer}>
          <div className={styles.label}>
            Monitor
          </div>

          <FormInputGroup
            type={inputTypes.MOVIE_MONITORED_SELECT}
            name="monitor"
            value={monitor}
            isDisabled={!selectedCount}
            includeMixed={isMonitorMixed}
            onChange={this.onInputChange}
          />
        </div>

        <div className={styles.inputContainer}>
          <div className={styles.label}>
            Quality Profile
          </div>

          <FormInputGroup
            type={inputTypes.QUALITY_PROFILE_SELECT}
            name="qualityProfileId"
            value={qualityProfileId}
            isDisabled={!selectedCount}
            includeMixed={isQualityProfileIdMixed}
            onChange={this.onInputChange}
          />
        </div>

        <div>
          <div className={styles.label}>
            &nbsp;
          </div>

          <div className={styles.importButtonContainer}>
            <SpinnerButton
              className={styles.importButton}
              kind={kinds.PRIMARY}
              isSpinning={isImporting}
              isDisabled={!selectedCount || isLookingUpMovie}
              onPress={onImportPress}
            >
              Import {selectedCount} {selectedCount > 1 ? 'Movies' : 'Movie'}
            </SpinnerButton>

            {
              isLookingUpMovie &&
                <Button
                  className={styles.loadingButton}
                  kind={kinds.WARNING}
                  onPress={onCancelLookupPress}
                >
                  Cancel Processing
                </Button>
            }

            {
              isLookingUpMovie &&
                <LoadingIndicator
                  className={styles.loading}
                  size={24}
                />
            }

            {
              isLookingUpMovie &&
                'Processing Folders'
            }
          </div>
        </div>
      </PageContentFooter>
    );
  }
}

ImportMovieFooter.propTypes = {
  selectedCount: PropTypes.number.isRequired,
  isImporting: PropTypes.bool.isRequired,
  isLookingUpMovie: PropTypes.bool.isRequired,
  defaultMonitor: PropTypes.string.isRequired,
  defaultQualityProfileId: PropTypes.number,
  isMonitorMixed: PropTypes.bool.isRequired,
  isQualityProfileIdMixed: PropTypes.bool.isRequired,
  onInputChange: PropTypes.func.isRequired,
  onImportPress: PropTypes.func.isRequired,
  onCancelLookupPress: PropTypes.func.isRequired
};

export default ImportMovieFooter;
