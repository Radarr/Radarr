import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Label from 'Components/Label';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { inputTypes, kinds, sizes } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './TagsModalContent.css';

class TagsModalContent extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      tags: [],
      applyTags: 'add'
    };
  }

  //
  // Lifecycle

  onInputChange = ({ name, value }) => {
    this.setState({ [name]: value });
  };

  onApplyTagsPress = () => {
    const {
      tags,
      applyTags
    } = this.state;

    this.props.onApplyTagsPress(tags, applyTags);
  };

  //
  // Render

  render() {
    const {
      movieTags,
      tagList,
      onModalClose
    } = this.props;

    const {
      tags,
      applyTags
    } = this.state;

    const applyTagsOptions = [
      { key: 'add', value: translate('Add') },
      { key: 'remove', value: translate('Remove') },
      { key: 'replace', value: translate('Replace') }
    ];

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          Tags
        </ModalHeader>

        <ModalBody>
          <Form>
            <FormGroup>
              <FormLabel>{translate('Tags')}</FormLabel>

              <FormInputGroup
                type={inputTypes.TAG}
                name="tags"
                value={tags}
                onChange={this.onInputChange}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{translate('ApplyTags')}</FormLabel>

              <FormInputGroup
                type={inputTypes.SELECT}
                name="applyTags"
                value={applyTags}
                values={applyTagsOptions}
                helpTexts={[
                  translate('ApplyTagsHelpTexts1'),
                  translate('ApplyTagsHelpTexts2'),
                  translate('ApplyTagsHelpTexts3'),
                  translate('ApplyTagsHelpTexts4')
                ]}
                onChange={this.onInputChange}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{translate('Result')}</FormLabel>

              <div className={styles.result}>
                {
                  movieTags.map((t) => {
                    const tag = _.find(tagList, { id: t });

                    if (!tag) {
                      return null;
                    }

                    const removeTag = (applyTags === 'remove' && tags.indexOf(t) > -1) ||
                        (applyTags === 'replace' && tags.indexOf(t) === -1);

                    return (
                      <Label
                        key={tag.id}
                        title={removeTag ? translate('RemovingTag') : translate('ExistingTag')}
                        kind={removeTag ? kinds.INVERSE : kinds.INFO}
                        size={sizes.LARGE}
                      >
                        {tag.label}
                      </Label>
                    );
                  })
                }

                {
                  (applyTags === 'add' || applyTags === 'replace') &&
                      tags.map((t) => {
                        const tag = _.find(tagList, { id: t });

                        if (!tag) {
                          return null;
                        }

                        if (movieTags.indexOf(t) > -1) {
                          return null;
                        }

                        return (
                          <Label
                            key={tag.id}
                            title={translate('AddingTag')}
                            kind={kinds.SUCCESS}
                            size={sizes.LARGE}
                          >
                            {tag.label}
                          </Label>
                        );
                      })
                }
              </div>
            </FormGroup>
          </Form>
        </ModalBody>

        <ModalFooter>
          <Button onPress={onModalClose}>
            {translate('Cancel')}
          </Button>

          <Button
            kind={kinds.PRIMARY}
            onPress={this.onApplyTagsPress}
          >
            {translate('Apply')}
          </Button>
        </ModalFooter>
      </ModalContent>
    );
  }
}

TagsModalContent.propTypes = {
  movieTags: PropTypes.arrayOf(PropTypes.number).isRequired,
  tagList: PropTypes.arrayOf(PropTypes.object).isRequired,
  onModalClose: PropTypes.func.isRequired,
  onApplyTagsPress: PropTypes.func.isRequired
};

export default TagsModalContent;
