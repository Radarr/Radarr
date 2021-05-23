import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Button from 'Components/Link/Button';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { inputTypes, kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';

class FileEditModalContent extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    const {
      qualityId,
      languageIds,
      indexerFlags,
      proper,
      real,
      edition,
      releaseGroup
    } = props;

    this.state = {
      qualityId,
      languageIds,
      indexerFlags,
      proper,
      real,
      edition,
      releaseGroup
    };
  }

  //
  // Listeners

  onQualityChange = ({ value }) => {
    this.setState({ qualityId: parseInt(value) });
  }

  onInputChange = ({ name, value }) => {
    this.setState({ [name]: value });
  }

  onSaveInputs = () => {
    this.props.onSaveInputs(this.state);
  }

  //
  // Render

  render() {
    const {
      isFetching,
      isPopulated,
      error,
      qualities,
      languages,
      relativePath,
      onModalClose
    } = this.props;

    const {
      qualityId,
      languageIds,
      indexerFlags,
      proper,
      real,
      edition,
      releaseGroup
    } = this.state;

    const qualityOptions = qualities.map(({ id, name }) => {
      return {
        key: id,
        value: name
      };
    });

    const languageOptions = languages.map(({ id, name }) => {
      return {
        key: id,
        value: name
      };
    });

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          {translate('EditMovieFile')} - {relativePath}
        </ModalHeader>

        <ModalBody>
          {
            isFetching &&
              <LoadingIndicator />
          }

          {
            !isFetching && !!error &&
              <div>
                {translate('UnableToLoadQualities')}
              </div>
          }

          {
            isPopulated && !error &&
              <Form>
                <FormGroup>
                  <FormLabel>{translate('Quality')}</FormLabel>

                  <FormInputGroup
                    type={inputTypes.SELECT}
                    name="quality"
                    value={qualityId}
                    values={qualityOptions}
                    onChange={this.onQualityChange}
                  />
                </FormGroup>

                <FormGroup>
                  <FormLabel>{translate('Proper')}</FormLabel>

                  <FormInputGroup
                    type={inputTypes.CHECK}
                    name="proper"
                    value={proper}
                    onChange={this.onInputChange}
                  />
                </FormGroup>

                <FormGroup>
                  <FormLabel>{translate('Real')}</FormLabel>

                  <FormInputGroup
                    type={inputTypes.CHECK}
                    name="real"
                    value={real}
                    onChange={this.onInputChange}
                  />
                </FormGroup>

                <FormGroup>
                  <FormLabel>{translate('Languages')}</FormLabel>

                  <FormInputGroup
                    type={inputTypes.LANGUAGE_SELECT}
                    name="languageIds"
                    value={languageIds}
                    values={languageOptions}
                    onChange={this.onInputChange}
                  />
                </FormGroup>

                <FormGroup>
                  <FormLabel>{translate('IndexerFlags')}</FormLabel>

                  <FormInputGroup
                    type={inputTypes.INDEXER_FLAGS_SELECT}
                    name="indexerFlags"
                    indexerFlags={indexerFlags}
                    onChange={this.onInputChange}
                  />
                </FormGroup>

                <FormGroup>
                  <FormLabel>{translate('Edition')}</FormLabel>

                  <FormInputGroup
                    type={inputTypes.TEXT}
                    name="edition"
                    value={edition}
                    onChange={this.onInputChange}
                  />
                </FormGroup>

                <FormGroup>
                  <FormLabel>{translate('ReleaseGroup')}</FormLabel>

                  <FormInputGroup
                    type={inputTypes.TEXT}
                    name="releaseGroup"
                    value={releaseGroup}
                    onChange={this.onInputChange}
                  />
                </FormGroup>
              </Form>
          }
        </ModalBody>

        <ModalFooter>
          <Button onPress={onModalClose}>
            {translate('Cancel')}
          </Button>

          <Button
            kind={kinds.SUCCESS}
            onPress={this.onSaveInputs}
          >
            {translate('Save')}
          </Button>
        </ModalFooter>
      </ModalContent>
    );
  }
}

FileEditModalContent.propTypes = {
  qualityId: PropTypes.number.isRequired,
  proper: PropTypes.bool.isRequired,
  real: PropTypes.bool.isRequired,
  relativePath: PropTypes.string.isRequired,
  edition: PropTypes.string.isRequired,
  releaseGroup: PropTypes.string.isRequired,
  languageIds: PropTypes.arrayOf(PropTypes.number).isRequired,
  languages: PropTypes.arrayOf(PropTypes.object).isRequired,
  indexerFlags: PropTypes.number.isRequired,
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  qualities: PropTypes.arrayOf(PropTypes.object).isRequired,
  onSaveInputs: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default FileEditModalContent;
