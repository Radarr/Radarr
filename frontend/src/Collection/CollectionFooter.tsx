import React, { useCallback, useEffect, useState } from 'react';
import { Error } from 'App/State/AppSectionState';
import FormInputGroup from 'Components/Form/FormInputGroup';
import { EnhancedSelectInputValue } from 'Components/Form/Select/EnhancedSelectInput';
import SpinnerButton from 'Components/Link/SpinnerButton';
import PageContentFooter from 'Components/Page/PageContentFooter';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { inputTypes, kinds } from 'Helpers/Props';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import CollectionFooterLabel from './CollectionFooterLabel';
import styles from './CollectionFooter.css';

interface SavePayload {
  monitored?: boolean;
  monitor?: string;
  qualityProfileId?: number;
  minimumAvailability?: string;
  rootFolderPath?: string;
  searchOnAdd?: boolean;
}

interface CollectionFooterProps {
  selectedIds: number[];
  isAdding: boolean;
  isSaving: boolean;
  saveError: Error;
  onUpdateSelectedPress(payload: object): void;
}

const NO_CHANGE = 'noChange';

const monitoredOptions: EnhancedSelectInputValue<string>[] = [
  {
    key: NO_CHANGE,
    get value() {
      return translate('NoChange');
    },
  },
  {
    key: 'monitored',
    get value() {
      return translate('Monitored');
    },
  },
  {
    key: 'unmonitored',
    get value() {
      return translate('Unmonitored');
    },
  },
];

const searchOnAddOptions: EnhancedSelectInputValue<string>[] = [
  {
    key: NO_CHANGE,
    get value() {
      return translate('NoChange');
    },
  },
  {
    key: 'yes',
    get value() {
      return translate('Yes');
    },
  },
  {
    key: 'no',
    get value() {
      return translate('No');
    },
  },
];

function CollectionFooter({
  selectedIds,
  isSaving,
  saveError,
  onUpdateSelectedPress,
}: CollectionFooterProps) {
  const [monitored, setMonitored] = useState(NO_CHANGE);
  const [monitor, setMonitor] = useState(NO_CHANGE);
  const [qualityProfileId, setQualityProfileId] = useState<string | number>(
    NO_CHANGE
  );
  const [minimumAvailability, setMinimumAvailability] = useState(NO_CHANGE);
  const [rootFolderPath, setRootFolderPath] = useState(NO_CHANGE);
  const [searchOnAdd, setSearchOnAdd] = useState(NO_CHANGE);

  const wasSaving = usePrevious(isSaving);

  const handleSavePress = useCallback(() => {
    let hasChanges = false;
    const payload: SavePayload = {};

    if (monitored !== NO_CHANGE) {
      hasChanges = true;
      payload.monitored = monitored === 'monitored';
    }

    if (monitor !== NO_CHANGE) {
      hasChanges = true;
      payload.monitor = monitor;
    }

    if (qualityProfileId !== NO_CHANGE) {
      hasChanges = true;
      payload.qualityProfileId = qualityProfileId as number;
    }

    if (minimumAvailability !== NO_CHANGE) {
      hasChanges = true;
      payload.minimumAvailability = minimumAvailability as string;
    }

    if (rootFolderPath !== NO_CHANGE) {
      hasChanges = true;
      payload.rootFolderPath = rootFolderPath;
    }

    if (searchOnAdd !== NO_CHANGE) {
      hasChanges = true;
      payload.searchOnAdd = searchOnAdd === 'yes';
    }

    if (hasChanges) {
      onUpdateSelectedPress(payload);
    }
  }, [
    monitor,
    monitored,
    qualityProfileId,
    minimumAvailability,
    rootFolderPath,
    searchOnAdd,
    onUpdateSelectedPress,
  ]);

  const handleInputChange = useCallback(({ name, value }: InputChanged) => {
    switch (name) {
      case 'monitored':
        setMonitored(value as string);
        break;
      case 'monitor':
        setMonitor(value as string);
        break;
      case 'qualityProfileId':
        setQualityProfileId(value as string);
        break;
      case 'minimumAvailability':
        setMinimumAvailability(value as string);
        break;
      case 'rootFolderPath':
        setRootFolderPath(value as string);
        break;
      case 'searchOnAdd':
        setSearchOnAdd(value as string);
        break;
      default:
        console.warn(`CollectionFooter Unknown Input: '${name}'`);
    }
  }, []);

  useEffect(() => {
    if (!isSaving && wasSaving && !saveError) {
      setMonitored(NO_CHANGE);
      setMonitor(NO_CHANGE);
      setQualityProfileId(NO_CHANGE);
      setMinimumAvailability(NO_CHANGE);
      setRootFolderPath(NO_CHANGE);
      setSearchOnAdd(NO_CHANGE);
    }
  }, [
    isSaving,
    wasSaving,
    saveError,
    setMonitored,
    setMonitor,
    setQualityProfileId,
    setMinimumAvailability,
    setRootFolderPath,
    setSearchOnAdd,
  ]);

  const selectedCount = selectedIds.length;

  return (
    <PageContentFooter>
      <div className={styles.inputContainer}>
        <CollectionFooterLabel
          label={translate('MonitorCollection')}
          isSaving={isSaving && monitored !== NO_CHANGE}
        />

        <FormInputGroup
          type={inputTypes.SELECT}
          name="monitored"
          value={monitored}
          values={monitoredOptions}
          isDisabled={!selectedCount}
          onChange={handleInputChange}
        />
      </div>

      <div className={styles.inputContainer}>
        <CollectionFooterLabel
          label={translate('MonitorMovies')}
          isSaving={isSaving && monitor !== NO_CHANGE}
        />

        <FormInputGroup
          type={inputTypes.SELECT}
          name="monitor"
          value={monitor}
          values={monitoredOptions}
          isDisabled={!selectedCount}
          onChange={handleInputChange}
        />
      </div>

      <div className={styles.inputContainer}>
        <CollectionFooterLabel
          label={translate('QualityProfile')}
          isSaving={isSaving && qualityProfileId !== NO_CHANGE}
        />

        <FormInputGroup
          type={inputTypes.QUALITY_PROFILE_SELECT}
          name="qualityProfileId"
          value={qualityProfileId}
          includeNoChange={true}
          includeNoChangeDisabled={false}
          isDisabled={!selectedCount}
          onChange={handleInputChange}
        />
      </div>

      <div className={styles.inputContainer}>
        <CollectionFooterLabel
          label={translate('MinimumAvailability')}
          isSaving={isSaving && minimumAvailability !== NO_CHANGE}
        />

        <FormInputGroup
          type={inputTypes.AVAILABILITY_SELECT}
          name="minimumAvailability"
          value={minimumAvailability}
          includeNoChange={true}
          includeNoChangeDisabled={false}
          isDisabled={!selectedCount}
          onChange={handleInputChange}
        />
      </div>

      <div className={styles.inputContainer}>
        <CollectionFooterLabel
          label={translate('RootFolder')}
          isSaving={isSaving && rootFolderPath !== NO_CHANGE}
        />

        <FormInputGroup
          type={inputTypes.ROOT_FOLDER_SELECT}
          name="rootFolderPath"
          value={rootFolderPath}
          includeNoChange={true}
          includeNoChangeDisabled={false}
          selectedValueOptions={{ includeFreeSpace: false }}
          isDisabled={!selectedCount}
          onChange={handleInputChange}
        />
      </div>

      <div className={styles.inputContainer}>
        <CollectionFooterLabel
          label={translate('SearchMoviesOnAdd')}
          isSaving={isSaving && searchOnAdd !== NO_CHANGE}
        />

        <FormInputGroup
          type={inputTypes.SELECT}
          name="searchOnAdd"
          value={searchOnAdd}
          values={searchOnAddOptions}
          isDisabled={!selectedCount}
          onChange={handleInputChange}
        />
      </div>

      <div className={styles.buttonContainer}>
        <div className={styles.buttonContainerContent}>
          <CollectionFooterLabel
            label={translate('CountCollectionsSelected', {
              count: selectedCount,
            })}
            isSaving={false}
          />

          <div className={styles.buttons}>
            <div>
              <SpinnerButton
                className={styles.addSelectedButton}
                kind={kinds.PRIMARY}
                isSpinning={isSaving}
                isDisabled={!selectedCount || isSaving}
                onPress={handleSavePress}
              >
                {translate('UpdateSelected')}
              </SpinnerButton>
            </div>
          </div>
        </div>
      </div>
    </PageContentFooter>
  );
}

export default CollectionFooter;
