import React, { useCallback, useEffect, useMemo } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import useMovieCollection from 'Collection/useMovieCollection';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { inputTypes } from 'Helpers/Props';
import MoviePoster from 'Movie/MoviePoster';
import {
  saveMovieCollection,
  setMovieCollectionValue,
} from 'Store/Actions/movieCollectionActions';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import selectSettings from 'Store/Selectors/selectSettings';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import styles from './EditMovieCollectionModalContent.css';

export interface EditMovieCollectionModalContentProps {
  collectionId: number;
  onModalClose: () => void;
}

function EditMovieCollectionModalContent({
  collectionId,
  onModalClose,
}: EditMovieCollectionModalContentProps) {
  const dispatch = useDispatch();

  const {
    title,
    overview,
    monitored,
    qualityProfileId,
    minimumAvailability,
    rootFolderPath,
    searchOnAdd,
    images,
    tags,
  } = useMovieCollection(collectionId)!;

  const { isSaving, saveError, pendingChanges } = useSelector(
    (state: AppState) => state.movieCollections
  );
  const { isSmallScreen } = useSelector(createDimensionsSelector());

  const wasSaving = usePrevious(isSaving);

  const { settings, ...otherSettings } = useMemo(() => {
    return selectSettings(
      {
        monitored,
        minimumAvailability,
        qualityProfileId,
        rootFolderPath,
        searchOnAdd,
        tags,
      },
      pendingChanges,
      saveError
    );
  }, [
    monitored,
    minimumAvailability,
    qualityProfileId,
    rootFolderPath,
    searchOnAdd,
    tags,
    pendingChanges,
    saveError,
  ]);

  const handleInputChange = useCallback(
    ({ name, value }: InputChanged) => {
      // @ts-expect-error actions aren't typed
      dispatch(setMovieCollectionValue({ name, value }));
    },
    [dispatch]
  );

  const handleSavePress = useCallback(() => {
    dispatch(
      saveMovieCollection({
        id: collectionId,
      })
    );
  }, [collectionId, dispatch]);

  useEffect(() => {
    if (!isSaving && wasSaving && !saveError) {
      onModalClose();
    }
  }, [isSaving, wasSaving, saveError, onModalClose]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {translate('EditMovieCollectionModalHeader', { title })}
      </ModalHeader>

      <ModalBody>
        <div className={styles.container}>
          {isSmallScreen ? null : (
            <div className={styles.poster}>
              <MoviePoster
                className={styles.poster}
                images={images}
                size={250}
              />
            </div>
          )}

          <div className={styles.info}>
            <div className={styles.overview}>{overview}</div>

            <Form {...otherSettings}>
              <FormGroup>
                <FormLabel>{translate('Monitored')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="monitored"
                  helpText={translate('MonitoredCollectionHelpText')}
                  {...settings.monitored}
                  onChange={handleInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>{translate('MinimumAvailability')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.AVAILABILITY_SELECT}
                  name="minimumAvailability"
                  {...settings.minimumAvailability}
                  onChange={handleInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>{translate('QualityProfile')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.QUALITY_PROFILE_SELECT}
                  name="qualityProfileId"
                  {...settings.qualityProfileId}
                  onChange={handleInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>{translate('RootFolder')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.ROOT_FOLDER_SELECT}
                  name="rootFolderPath"
                  {...settings.rootFolderPath}
                  includeMissingValue={true}
                  onChange={handleInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>{translate('Tags')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.TAG}
                  name="tags"
                  {...settings.tags}
                  onChange={handleInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>{translate('SearchOnAdd')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="searchOnAdd"
                  helpText={translate('SearchOnAddCollectionHelpText')}
                  {...settings.searchOnAdd}
                  onChange={handleInputChange}
                />
              </FormGroup>
            </Form>
          </div>
        </div>
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Cancel')}</Button>

        <SpinnerErrorButton
          error={saveError}
          isSpinning={isSaving}
          onPress={handleSavePress}
        >
          {translate('Save')}
        </SpinnerErrorButton>
      </ModalFooter>
    </ModalContent>
  );
}

export default EditMovieCollectionModalContent;
