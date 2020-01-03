import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { inputTypes, kinds, sizes } from 'Helpers/Props';
import Button from 'Components/Link/Button';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormLabel from 'Components/Form/FormLabel';
import FormInputGroup from 'Components/Form/FormInputGroup';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';
import styles from './SelectLanguageModalContent.css';

class SelectLanguageModalContent extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    const {
      languageIds
    } = props;

    this.state = {
      languageIds
    };
  }

  //
  // Listeners

  onLanguageChange = ({ value, name }) => {
    const {
      languageIds
    } = this.state;

    const changedId = parseInt(name);

    let newLanguages = languageIds;

    if (value) {
      newLanguages.push(changedId);
    }

    if (!value) {
      newLanguages = languageIds.filter((i) => i !== changedId);
    }

    this.setState({ languageIds: newLanguages });
  }

  onLanguageSelect = () => {
    this.props.onLanguageSelect(this.state);
  }

  //
  // Render

  render() {
    const {
      isFetching,
      isPopulated,
      error,
      items,
      onModalClose
    } = this.props;

    const {
      languageIds
    } = this.state;

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          Manual Import - Select Language
        </ModalHeader>

        <ModalBody>
          {
            isFetching &&
              <LoadingIndicator />
          }

          {
            !isFetching && !!error &&
              <div>Unable to load languages</div>
          }

          {
            isPopulated && !error &&
              <Form>
                {
                  items.map(( language ) => {
                    return (
                      <FormGroup
                        key={language.id}
                        size={sizes.EXTRA_SMALL}
                        className={styles.languageInput}
                      >
                        <FormLabel>{language.name}</FormLabel>
                        <FormInputGroup
                          type={inputTypes.CHECK}
                          name={language.id.toString()}
                          value={languageIds.includes(language.id)}
                          onChange={this.onLanguageChange}
                        />
                      </FormGroup>
                    );
                  })
                }
              </Form>
          }
        </ModalBody>

        <ModalFooter>
          <Button onPress={onModalClose}>
            Cancel
          </Button>

          <Button
            kind={kinds.SUCCESS}
            onPress={this.onLanguageSelect}
          >
            Select Languges
          </Button>
        </ModalFooter>
      </ModalContent>
    );
  }
}

SelectLanguageModalContent.propTypes = {
  languageIds: PropTypes.arrayOf(PropTypes.number).isRequired,
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  onLanguageSelect: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

SelectLanguageModalContent.defaultProps = {
  languages: []
};

export default SelectLanguageModalContent;
