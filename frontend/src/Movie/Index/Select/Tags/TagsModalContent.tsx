import { uniq } from 'lodash';
import React, { useCallback, useMemo, useState } from 'react';
import { useSelector } from 'react-redux';
import { Tag } from 'App/State/TagsAppState';
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
import Movie from 'Movie/Movie';
import createAllMoviesSelector from 'Store/Selectors/createAllMoviesSelector';
import createTagsSelector from 'Store/Selectors/createTagsSelector';
import translate from 'Utilities/String/translate';
import styles from './TagsModalContent.css';

interface TagsModalContentProps {
  movieIds: number[];
  onApplyTagsPress: (tags: number[], applyTags: string) => void;
  onModalClose: () => void;
}

function TagsModalContent(props: TagsModalContentProps) {
  const { movieIds, onModalClose, onApplyTagsPress } = props;

  const allMovies: Movie[] = useSelector(createAllMoviesSelector());
  const tagList: Tag[] = useSelector(createTagsSelector());

  const [tags, setTags] = useState<number[]>([]);
  const [applyTags, setApplyTags] = useState('add');

  const movieTags = useMemo(() => {
    const tags = movieIds.reduce((acc: number[], id) => {
      const s = allMovies.find((s) => s.id === id);

      if (s) {
        acc.push(...s.tags);
      }

      return acc;
    }, []);

    return uniq(tags);
  }, [movieIds, allMovies]);

  const onTagsChange = useCallback(
    ({ value }: { value: number[] }) => {
      setTags(value);
    },
    [setTags]
  );

  const onApplyTagsChange = useCallback(
    ({ value }: { value: string }) => {
      setApplyTags(value);
    },
    [setApplyTags]
  );

  const onApplyPress = useCallback(() => {
    onApplyTagsPress(tags, applyTags);
  }, [tags, applyTags, onApplyTagsPress]);

  const applyTagsOptions = [
    { key: 'add', value: translate('Add') },
    { key: 'remove', value: translate('Remove') },
    { key: 'replace', value: translate('Replace') },
  ];

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('Tags')}</ModalHeader>

      <ModalBody>
        <Form>
          <FormGroup>
            <FormLabel>{translate('Tags')}</FormLabel>

            <FormInputGroup
              type={inputTypes.TAG}
              name="tags"
              value={tags}
              onChange={onTagsChange}
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
                translate('ApplyTagsHelpTextHowToApplyMovies'),
                translate('ApplyTagsHelpTextAdd'),
                translate('ApplyTagsHelpTextRemove'),
                translate('ApplyTagsHelpTextReplace'),
              ]}
              onChange={onApplyTagsChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('Result')}</FormLabel>

            <div className={styles.result}>
              {movieTags.map((id) => {
                const tag = tagList.find((t) => t.id === id);

                if (!tag) {
                  return null;
                }

                const removeTag =
                  (applyTags === 'remove' && tags.indexOf(id) > -1) ||
                  (applyTags === 'replace' && tags.indexOf(id) === -1);

                return (
                  <Label
                    key={tag.id}
                    title={
                      removeTag
                        ? translate('RemovingTag')
                        : translate('ExistingTag')
                    }
                    kind={removeTag ? kinds.INVERSE : kinds.INFO}
                    size={sizes.LARGE}
                  >
                    {tag.label}
                  </Label>
                );
              })}

              {(applyTags === 'add' || applyTags === 'replace') &&
                tags.map((id) => {
                  const tag = tagList.find((t) => t.id === id);

                  if (!tag) {
                    return null;
                  }

                  if (movieTags.indexOf(id) > -1) {
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
                })}
            </div>
          </FormGroup>
        </Form>
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Cancel')}</Button>

        <Button kind={kinds.PRIMARY} onPress={onApplyPress}>
          {translate('Apply')}
        </Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default TagsModalContent;
