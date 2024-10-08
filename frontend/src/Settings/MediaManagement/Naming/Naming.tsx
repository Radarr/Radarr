import React, { useCallback, useEffect, useRef, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import Alert from 'Components/Alert';
import FieldSet from 'Components/FieldSet';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputButton from 'Components/Form/FormInputButton';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import useModalOpenState from 'Helpers/Hooks/useModalOpenState';
import { inputTypes, kinds, sizes } from 'Helpers/Props';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import {
  fetchNamingExamples,
  fetchNamingSettings,
  setNamingSettingsValue,
} from 'Store/Actions/settingsActions';
import createSettingsSectionSelector from 'Store/Selectors/createSettingsSectionSelector';
import NamingConfig from 'typings/Settings/NamingConfig';
import translate from 'Utilities/String/translate';
import NamingModal from './NamingModal';
import styles from './Naming.css';

const SECTION = 'naming';

function createNamingSelector() {
  return createSelector(
    (state: AppState) => state.settings.advancedSettings,
    (state: AppState) => state.settings.namingExamples,
    createSettingsSectionSelector(SECTION),
    (advancedSettings, namingExamples, sectionSettings) => {
      return {
        advancedSettings,
        examples: namingExamples.item,
        examplesPopulated: namingExamples.isPopulated,
        ...sectionSettings,
      };
    }
  );
}

interface NamingModalOptions {
  name: keyof Pick<NamingConfig, 'standardMovieFormat' | 'movieFolderFormat'>;
  movie?: boolean;
  additional?: boolean;
}

function Naming() {
  const {
    advancedSettings,
    isFetching,
    error,
    settings,
    hasSettings,
    examples,
    examplesPopulated,
  } = useSelector(createNamingSelector());

  const dispatch = useDispatch();

  const [isNamingModalOpen, setNamingModalOpen, setNamingModalClosed] =
    useModalOpenState(false);
  const [namingModalOptions, setNamingModalOptions] =
    useState<NamingModalOptions | null>(null);
  const namingExampleTimeout = useRef<ReturnType<typeof setTimeout>>();

  useEffect(() => {
    dispatch(fetchNamingSettings());
    dispatch(fetchNamingExamples());

    return () => {
      dispatch(clearPendingChanges({ section: SECTION }));
    };
  }, [dispatch]);

  const handleInputChange = useCallback(
    ({ name, value }: { name: string; value: string }) => {
      // @ts-expect-error 'setNamingSettingsValue' isn't typed yet
      dispatch(setNamingSettingsValue({ name, value }));

      if (namingExampleTimeout.current) {
        clearTimeout(namingExampleTimeout.current);
      }

      namingExampleTimeout.current = setTimeout(() => {
        dispatch(fetchNamingExamples());
      }, 1000);
    },
    [dispatch]
  );

  const onStandardNamingModalOpenClick = useCallback(() => {
    setNamingModalOpen();

    setNamingModalOptions({
      name: 'standardMovieFormat',
      movie: true,
      additional: true,
    });
  }, [setNamingModalOpen, setNamingModalOptions]);

  const onMovieFolderNamingModalOpenClick = useCallback(() => {
    setNamingModalOpen();

    setNamingModalOptions({
      name: 'movieFolderFormat',
    });
  }, [setNamingModalOpen, setNamingModalOptions]);

  const renameMovies = hasSettings && settings.renameMovies.value;
  const replaceIllegalCharacters =
    hasSettings && settings.replaceIllegalCharacters.value;

  const colonReplacementOptions = [
    { key: 'delete', value: translate('Delete') },
    { key: 'dash', value: translate('ReplaceWithDash') },
    { key: 'spaceDash', value: translate('ReplaceWithSpaceDash') },
    { key: 'spaceDashSpace', value: translate('ReplaceWithSpaceDashSpace') },
    {
      key: 'smart',
      value: translate('SmartReplace'),
      hint: translate('SmartReplaceHint'),
    },
  ];

  const standardMovieFormatHelpTexts = [];
  const standardMovieFormatErrors = [];
  const movieFolderFormatHelpTexts = [];
  const movieFolderFormatErrors = [];

  if (examplesPopulated) {
    if (examples.movieExample) {
      standardMovieFormatHelpTexts.push(
        `${translate('Movie')}: ${examples.movieExample}`
      );
    } else {
      standardMovieFormatErrors.push({
        message: translate('MovieInvalidFormat'),
      });
    }

    if (examples.movieFolderExample) {
      movieFolderFormatHelpTexts.push(
        `${translate('Example')}: ${examples.movieFolderExample}`
      );
    } else {
      movieFolderFormatErrors.push({ message: translate('InvalidFormat') });
    }
  }

  return (
    <FieldSet legend={translate('MovieNaming')}>
      {isFetching ? <LoadingIndicator /> : null}

      {!isFetching && error ? (
        <Alert kind={kinds.DANGER}>
          {translate('NamingSettingsLoadError')}
        </Alert>
      ) : null}

      {hasSettings && !isFetching && !error ? (
        <Form>
          <FormGroup size={sizes.MEDIUM}>
            <FormLabel>{translate('RenameMovies')}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="renameMovies"
              helpText={translate('RenameMoviesHelpText')}
              onChange={handleInputChange}
              {...settings.renameMovies}
            />
          </FormGroup>

          <FormGroup size={sizes.MEDIUM}>
            <FormLabel>{translate('ReplaceIllegalCharacters')}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="replaceIllegalCharacters"
              helpText={translate('ReplaceIllegalCharactersHelpText')}
              onChange={handleInputChange}
              {...settings.replaceIllegalCharacters}
            />
          </FormGroup>

          {replaceIllegalCharacters ? (
            <FormGroup size={sizes.MEDIUM}>
              <FormLabel>{translate('ColonReplacement')}</FormLabel>

              <FormInputGroup
                type={inputTypes.SELECT}
                name="colonReplacementFormat"
                values={colonReplacementOptions}
                helpText={translate('ColonReplacementFormatHelpText')}
                onChange={handleInputChange}
                {...settings.colonReplacementFormat}
              />
            </FormGroup>
          ) : null}

          {renameMovies ? (
            <FormGroup size={sizes.LARGE}>
              <FormLabel>{translate('StandardMovieFormat')}</FormLabel>

              <FormInputGroup
                inputClassName={styles.namingInput}
                type={inputTypes.TEXT}
                name="standardMovieFormat"
                buttons={
                  <FormInputButton onPress={onStandardNamingModalOpenClick}>
                    ?
                  </FormInputButton>
                }
                onChange={handleInputChange}
                {...settings.standardMovieFormat}
                helpTexts={standardMovieFormatHelpTexts}
                errors={[
                  ...standardMovieFormatErrors,
                  ...settings.standardMovieFormat.errors,
                ]}
              />
            </FormGroup>
          ) : null}

          <FormGroup
            advancedSettings={advancedSettings}
            isAdvanced={true}
            size={sizes.MEDIUM}
          >
            <FormLabel>{translate('MovieFolderFormat')}</FormLabel>

            <FormInputGroup
              inputClassName={styles.namingInput}
              type={inputTypes.TEXT}
              name="movieFolderFormat"
              buttons={
                <FormInputButton onPress={onMovieFolderNamingModalOpenClick}>
                  ?
                </FormInputButton>
              }
              onChange={handleInputChange}
              {...settings.movieFolderFormat}
              helpTexts={[
                translate('MovieFolderFormatHelpText'),
                ...movieFolderFormatHelpTexts,
              ]}
              errors={[
                ...movieFolderFormatErrors,
                ...settings.movieFolderFormat.errors,
              ]}
            />
          </FormGroup>

          {namingModalOptions ? (
            <NamingModal
              isOpen={isNamingModalOpen}
              {...namingModalOptions}
              value={settings[namingModalOptions.name].value}
              onInputChange={handleInputChange}
              onModalClose={setNamingModalClosed}
            />
          ) : null}
        </Form>
      ) : null}
    </FieldSet>
  );
}

export default Naming;
