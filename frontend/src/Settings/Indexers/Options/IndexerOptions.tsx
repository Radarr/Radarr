import React, { useCallback, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import Alert from 'Components/Alert';
import FieldSet from 'Components/FieldSet';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import useShowAdvancedSettings from 'Helpers/Hooks/useShowAdvancedSettings';
import { inputTypes, kinds } from 'Helpers/Props';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import {
  fetchIndexerOptions,
  saveIndexerOptions,
  setIndexerOptionsValue,
} from 'Store/Actions/settingsActions';
import createSettingsSectionSelector from 'Store/Selectors/createSettingsSectionSelector';
import { InputChanged } from 'typings/inputs';
import {
  OnChildStateChange,
  SetChildSave,
} from 'typings/Settings/SettingsState';
import translate from 'Utilities/String/translate';

const SECTION = 'indexerOptions';

interface IndexerOptionsProps {
  setChildSave: SetChildSave;
  onChildStateChange: OnChildStateChange;
}

function IndexerOptions({
  setChildSave,
  onChildStateChange,
}: IndexerOptionsProps) {
  const dispatch = useDispatch();
  const {
    isFetching,
    isPopulated,
    isSaving,
    error,
    settings,
    hasSettings,
    hasPendingChanges,
  } = useSelector(createSettingsSectionSelector(SECTION));

  const showAdvancedSettings = useShowAdvancedSettings();

  const handleInputChange = useCallback(
    (change: InputChanged) => {
      // @ts-expect-error - actions aren't typed
      dispatch(setIndexerOptionsValue(change));
    },
    [dispatch]
  );

  const handleWhitelistedSubtitleChange = useCallback(
    ({ name, value }: InputChanged<string[] | null>) => {
      // @ts-expect-error - actions aren't typed
      dispatch(setIndexerOptionsValue({ name, value: value?.join(',') }));
    },
    [dispatch]
  );

  useEffect(() => {
    dispatch(fetchIndexerOptions());
    setChildSave(() => dispatch(saveIndexerOptions()));
  }, [dispatch, setChildSave]);

  useEffect(() => {
    onChildStateChange({
      isSaving,
      hasPendingChanges,
    });
  }, [hasPendingChanges, isSaving, onChildStateChange]);

  useEffect(() => {
    return () => {
      dispatch(clearPendingChanges({ section: `settings.${SECTION}` }));
    };
  }, [dispatch]);

  return (
    <FieldSet legend={translate('Options')}>
      {isFetching ? <LoadingIndicator /> : null}

      {!isFetching && error ? (
        <Alert kind={kinds.DANGER}>
          {translate('IndexerOptionsLoadError')}
        </Alert>
      ) : null}

      {hasSettings && isPopulated && !error ? (
        <Form>
          <FormGroup>
            <FormLabel>{translate('MinimumAge')}</FormLabel>

            <FormInputGroup
              type={inputTypes.NUMBER}
              name="minimumAge"
              min={0}
              unit="minutes"
              helpText={translate('MinimumAgeHelpText')}
              onChange={handleInputChange}
              {...settings.minimumAge}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('Retention')}</FormLabel>

            <FormInputGroup
              type={inputTypes.NUMBER}
              name="retention"
              min={0}
              unit="days"
              helpText={translate('RetentionHelpText')}
              onChange={handleInputChange}
              {...settings.retention}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('MaximumSize')}</FormLabel>

            <FormInputGroup
              type={inputTypes.NUMBER}
              name="maximumSize"
              min={0}
              unit="MB"
              helpText={translate('MaximumSizeHelpText')}
              onChange={handleInputChange}
              {...settings.maximumSize}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('PreferIndexerFlags')}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="preferIndexerFlags"
              helpText={translate('PreferIndexerFlagsHelpText')}
              helpLink="https://wiki.servarr.com/radarr/settings#indexer-flags"
              onChange={handleInputChange}
              {...settings.preferIndexerFlags}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('AvailabilityDelay')}</FormLabel>

            <FormInputGroup
              type={inputTypes.NUMBER}
              name="availabilityDelay"
              unit="days"
              helpText={translate('AvailabilityDelayHelpText')}
              onChange={handleInputChange}
              {...settings.availabilityDelay}
            />
          </FormGroup>

          <FormGroup advancedSettings={showAdvancedSettings} isAdvanced={true}>
            <FormLabel>{translate('RssSyncInterval')}</FormLabel>

            <FormInputGroup
              type={inputTypes.NUMBER}
              name="rssSyncInterval"
              min={0}
              max={120}
              unit="minutes"
              helpText={translate('RssSyncIntervalHelpText')}
              helpTextWarning={translate('RssSyncIntervalHelpTextWarning')}
              helpLink="https://wiki.servarr.com/radarr/faq#how-does-radarr-work"
              onChange={handleInputChange}
              {...settings.rssSyncInterval}
            />
          </FormGroup>

          <FormGroup advancedSettings={showAdvancedSettings} isAdvanced={true}>
            <FormLabel>{translate('WhitelistedSubtitleTags')}</FormLabel>

            <FormInputGroup
              type={inputTypes.TEXT_TAG}
              name="whitelistedHardcodedSubs"
              helpText={translate('WhitelistedHardcodedSubsHelpText')}
              onChange={handleWhitelistedSubtitleChange}
              {...settings.whitelistedHardcodedSubs}
            />
          </FormGroup>

          <FormGroup advancedSettings={showAdvancedSettings} isAdvanced={true}>
            <FormLabel>{translate('AllowHardcodedSubs')}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="allowHardcodedSubs"
              helpText={translate('AllowHardcodedSubsHelpText')}
              onChange={handleInputChange}
              {...settings.allowHardcodedSubs}
            />
          </FormGroup>
        </Form>
      ) : null}
    </FieldSet>
  );
}

export default IndexerOptions;
