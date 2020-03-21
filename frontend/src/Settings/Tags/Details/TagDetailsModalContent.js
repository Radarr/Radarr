import PropTypes from 'prop-types';
import React from 'react';
import split from 'Utilities/String/split';
import { kinds } from 'Helpers/Props';
import FieldSet from 'Components/FieldSet';
import Button from 'Components/Link/Button';
import Label from 'Components/Label';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';
import TagDetailsDelayProfile from './TagDetailsDelayProfile';
import styles from './TagDetailsModalContent.css';

function TagDetailsModalContent(props) {
  const {
    label,
    isTagUsed,
    movies,
    delayProfiles,
    notifications,
    restrictions,
    netImports,
    onModalClose,
    onDeleteTagPress
  } = props;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        Tag Details - {label}
      </ModalHeader>

      <ModalBody>
        {
          !isTagUsed &&
            <div>Tag is not used and can be deleted</div>
        }

        {
          !!movies.length &&
            <FieldSet legend="Movies">
              {
                movies.map((item) => {
                  return (
                    <div key={item.id}>
                      {item.title}
                    </div>
                  );
                })
              }
            </FieldSet>
        }

        {
          !!delayProfiles.length &&
            <FieldSet legend="Delay Profile">
              {
                delayProfiles.map((item) => {
                  const {
                    id,
                    preferredProtocol,
                    enableUsenet,
                    enableTorrent,
                    usenetDelay,
                    torrentDelay
                  } = item;

                  return (
                    <TagDetailsDelayProfile
                      key={id}
                      preferredProtocol={preferredProtocol}
                      enableUsenet={enableUsenet}
                      enableTorrent={enableTorrent}
                      usenetDelay={usenetDelay}
                      torrentDelay={torrentDelay}
                    />
                  );
                })
              }
            </FieldSet>
        }

        {
          !!notifications.length &&
            <FieldSet legend="Connections">
              {
                notifications.map((item) => {
                  return (
                    <div key={item.id}>
                      {item.name}
                    </div>
                  );
                })
              }
            </FieldSet>
        }

        {
          !!restrictions.length &&
            <FieldSet legend="Restrictions">
              {
                restrictions.map((item) => {
                  return (
                    <div
                      key={item.id}
                      className={styles.restriction}
                    >
                      <div>
                        {
                          split(item.required).map((r) => {
                            return (
                              <Label
                                key={r}
                                kind={kinds.SUCCESS}
                              >
                                {r}
                              </Label>
                            );
                          })
                        }
                      </div>

                      <div>
                        {
                          split(item.ignored).map((i) => {
                            return (
                              <Label
                                key={i}
                                kind={kinds.DANGER}
                              >
                                {i}
                              </Label>
                            );
                          })
                        }
                      </div>
                    </div>
                  );
                })
              }
            </FieldSet>
        }

        {
          !!netImports.length &&
            <FieldSet legend="Lists">
              {
                netImports.map((item) => {
                  return (
                    <div key={item.id}>
                      {item.name}
                    </div>
                  );
                })
              }
            </FieldSet>
        }
      </ModalBody>

      <ModalFooter>
        {
          <Button
            className={styles.deleteButton}
            kind={kinds.DANGER}
            title={isTagUsed ? 'Cannot be deleted while in use' : undefined}
            isDisabled={isTagUsed}
            onPress={onDeleteTagPress}
          >
            Delete
          </Button>
        }

        <Button
          onPress={onModalClose}
        >
          Close
        </Button>
      </ModalFooter>
    </ModalContent>
  );
}

TagDetailsModalContent.propTypes = {
  label: PropTypes.string.isRequired,
  isTagUsed: PropTypes.bool.isRequired,
  movies: PropTypes.arrayOf(PropTypes.object).isRequired,
  delayProfiles: PropTypes.arrayOf(PropTypes.object).isRequired,
  notifications: PropTypes.arrayOf(PropTypes.object).isRequired,
  restrictions: PropTypes.arrayOf(PropTypes.object).isRequired,
  netImports: PropTypes.arrayOf(PropTypes.object).isRequired,
  onModalClose: PropTypes.func.isRequired,
  onDeleteTagPress: PropTypes.func.isRequired
};

export default TagDetailsModalContent;
