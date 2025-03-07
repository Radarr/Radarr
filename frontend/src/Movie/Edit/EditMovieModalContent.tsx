import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import MovieMinimumAvailabilityPopoverContent from 'AddMovie/MovieMinimumAvailabilityPopoverContent';
import AppState from 'App/State/AppState';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import Popover from 'Components/Tooltip/Popover';
import usePrevious from 'Helpers/Hooks/usePrevious';
import {
  icons,
  inputTypes,
  kinds,
  sizes,
  tooltipPositions,
} from 'Helpers/Props';
import MoveMovieModal from 'Movie/MoveMovie/MoveMovieModal';
import useMovie from 'Movie/useMovie';
import { saveMovie, setMovieValue } from 'Store/Actions/movieActions';
import selectSettings from 'Store/Selectors/selectSettings';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import styles from './EditMovieModalContent.css';

export interface EditMovieModalContentProps {
  movieId: number;
  onModalClose: () => void;
  onDeleteMoviePress: () => void;
}

function EditMovieModalContent({
  movieId,
  onModalClose,
  onDeleteMoviePress,
}: EditMovieModalContentProps) {
  const dispatch = useDispatch();
  const {
    title,
    monitored,
    minimumAvailability,
    qualityProfileId,
    path,
    tags,
  } = useMovie(movieId)!;

  const { isSaving, saveError, pendingChanges } = useSelector(
    (state: AppState) => state.movies
  );

  const wasSaving = usePrevious(isSaving);

  const isPathChanging = pendingChanges.path && path !== pendingChanges.path;

  const [isConfirmMoveModalOpen, setIsConfirmMoveModalOpen] = useState(false);

  const { settings, ...otherSettings } = useMemo(() => {
    return selectSettings(
      {
        monitored,
        minimumAvailability,
        qualityProfileId,
        path,
        tags,
      },
      pendingChanges,
      saveError
    );
  }, [
    monitored,
    minimumAvailability,
    qualityProfileId,
    path,
    tags,
    pendingChanges,
    saveError,
  ]);

  const handleInputChange = useCallback(
    ({ name, value }: InputChanged) => {
      // @ts-expect-error actions aren't typed
      dispatch(setMovieValue({ name, value }));
    },
    [dispatch]
  );

  const handleCancelPress = useCallback(() => {
    setIsConfirmMoveModalOpen(false);
  }, []);

  const handleSavePress = useCallback(() => {
    if (isPathChanging && !isConfirmMoveModalOpen) {
      setIsConfirmMoveModalOpen(true);
    } else {
      setIsConfirmMoveModalOpen(false);

      dispatch(
        saveMovie({
          id: movieId,
          moveFiles: false,
        })
      );
    }
  }, [movieId, isPathChanging, isConfirmMoveModalOpen, dispatch]);

  const handleMoveMoviePress = useCallback(() => {
    setIsConfirmMoveModalOpen(false);

    dispatch(
      saveMovie({
        id: movieId,
        moveFiles: true,
      })
    );
  }, [movieId, dispatch]);

  useEffect(() => {
    if (!isSaving && wasSaving && !saveError) {
      onModalClose();
    }
  }, [isSaving, wasSaving, saveError, onModalClose]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('EditMovieModalHeader', { title })}</ModalHeader>

      <ModalBody>
        <Form {...otherSettings}>
          <FormGroup size={sizes.MEDIUM}>
            <FormLabel>{translate('Monitored')}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="monitored"
              helpText={translate('MonitoredMovieHelpText')}
              {...settings.monitored}
              onChange={handleInputChange}
            />
          </FormGroup>

          <FormGroup size={sizes.MEDIUM}>
            <FormLabel>
              {translate('MinimumAvailability')}

              <Popover
                anchor={<Icon className={styles.labelIcon} name={icons.INFO} />}
                title={translate('MinimumAvailability')}
                body={<MovieMinimumAvailabilityPopoverContent />}
                position={tooltipPositions.RIGHT}
              />
            </FormLabel>

            <FormInputGroup
              type={inputTypes.AVAILABILITY_SELECT}
              name="minimumAvailability"
              {...settings.minimumAvailability}
              onChange={handleInputChange}
            />
          </FormGroup>

          <FormGroup size={sizes.MEDIUM}>
            <FormLabel>{translate('QualityProfile')}</FormLabel>

            <FormInputGroup
              type={inputTypes.QUALITY_PROFILE_SELECT}
              name="qualityProfileId"
              {...settings.qualityProfileId}
              onChange={handleInputChange}
            />
          </FormGroup>

          <FormGroup size={sizes.MEDIUM}>
            <FormLabel>{translate('Path')}</FormLabel>

            <FormInputGroup
              type={inputTypes.PATH}
              name="path"
              {...settings.path}
              onChange={handleInputChange}
            />
          </FormGroup>

          <FormGroup size={sizes.MEDIUM}>
            <FormLabel>{translate('Tags')}</FormLabel>

            <FormInputGroup
              type={inputTypes.TAG}
              name="tags"
              {...settings.tags}
              onChange={handleInputChange}
            />
          </FormGroup>
        </Form>
      </ModalBody>

      <ModalFooter>
        <Button
          className={styles.deleteButton}
          kind={kinds.DANGER}
          onPress={onDeleteMoviePress}
        >
          {translate('Delete')}
        </Button>

        <Button onPress={onModalClose}>{translate('Cancel')}</Button>

        <SpinnerErrorButton
          error={saveError}
          isSpinning={isSaving}
          onPress={handleSavePress}
        >
          {translate('Save')}
        </SpinnerErrorButton>
      </ModalFooter>

      <MoveMovieModal
        originalPath={path}
        destinationPath={pendingChanges.path}
        isOpen={isConfirmMoveModalOpen}
        onModalClose={handleCancelPress}
        onSavePress={handleSavePress}
        onMoveMoviePress={handleMoveMoviePress}
      />
    </ModalContent>
  );
}

export default EditMovieModalContent;
