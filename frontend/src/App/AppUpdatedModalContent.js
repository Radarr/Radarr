import PropTypes from 'prop-types';
import React from 'react';
import Button from 'Components/Link/Button';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { kinds } from 'Helpers/Props';
import UpdateChanges from 'System/Updates/UpdateChanges';
import styles from './AppUpdatedModalContent.css';

function AppUpdatedModalContent(props) {
  const {
    version,
    isPopulated,
    error,
    items,
    onSeeChangesPress,
    onModalClose
  } = props;

  const update = items[0];

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        Radarr Updated
      </ModalHeader>

      <ModalBody>
        <div>
          Version <span className={styles.version}>{version}</span> of Radarr has been installed, in order to get the latest changes you'll need to reload Radarr.
        </div>

        {
          isPopulated && !error && !!update &&
            <div>
              {
                !update.changes &&
                  <div className={styles.maintenance}>Maintenance release</div>
              }

              {
                !!update.changes &&
                  <div>
                    <div className={styles.changes}>
                      What's new?
                    </div>

                    <UpdateChanges
                      title="New"
                      changes={update.changes.new}
                    />

                    <UpdateChanges
                      title="Fixed"
                      changes={update.changes.fixed}
                    />
                  </div>
              }
            </div>
        }

        {
          !isPopulated && !error &&
            <LoadingIndicator />
        }
      </ModalBody>

      <ModalFooter>
        <Button
          onPress={onSeeChangesPress}
        >
          Recent Changes
        </Button>

        <Button
          kind={kinds.PRIMARY}
          onPress={onModalClose}
        >
          Reload
        </Button>
      </ModalFooter>
    </ModalContent>
  );
}

AppUpdatedModalContent.propTypes = {
  version: PropTypes.string.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  onSeeChangesPress: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default AppUpdatedModalContent;
