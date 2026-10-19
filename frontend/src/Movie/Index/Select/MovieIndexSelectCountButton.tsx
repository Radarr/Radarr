import React, {
  ChangeEvent,
  KeyboardEvent,
  useCallback,
  useState,
} from 'react';
import { useSelector } from 'react-redux';
import { useSelect } from 'App/SelectContext';
import ClientSideCollectionAppState from 'App/State/ClientSideCollectionAppState';
import MoviesAppState, { MovieIndexAppState } from 'App/State/MoviesAppState';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import Link from 'Components/Link/Link';
import Menu from 'Components/Menu/Menu';
import MenuContent from 'Components/Menu/MenuContent';
import MenuItem from 'Components/Menu/MenuItem';
import { icons, sizes } from 'Helpers/Props';
import createMovieClientSideCollectionItemsSelector from 'Store/Selectors/createMovieClientSideCollectionItemsSelector';
import translate from 'Utilities/String/translate';
import styles from './MovieIndexSelectCountButton.css';

const PRESETS = [250, 500, 1000];

interface MovieIndexSelectCountButtonProps {
  isSelectMode: boolean;
}

function parseCount(value: string) {
  const trimmedValue = value.trim();

  if (!/^\d+$/.test(trimmedValue)) {
    return null;
  }

  return parseInt(trimmedValue);
}

interface PresetMenuItemProps {
  preset: number;
  isDisabled: boolean;
  onPress: (preset: number) => void;
}

function PresetMenuItem(props: PresetMenuItemProps) {
  const { preset, isDisabled, onPress } = props;

  const handlePress = useCallback(() => {
    onPress(preset);
  }, [onPress, preset]);

  return (
    <MenuItem isDisabled={isDisabled} onPress={handlePress}>
      {preset}
    </MenuItem>
  );
}

function MovieIndexSelectCountButton(props: MovieIndexSelectCountButtonProps) {
  const { isSelectMode } = props;

  const {
    items,
  }: MoviesAppState & MovieIndexAppState & ClientSideCollectionAppState =
    useSelector(createMovieClientSideCollectionItemsSelector('movieIndex'));

  const [, selectDispatch] = useSelect();

  const [value, setValue] = useState('');
  const [committedValue, setCommittedValue] = useState('');
  const [hint, setHint] = useState<string | null>(null);

  const maxCount = items.length;
  const count = parseCount(value);
  const isCommittable = count !== null && count > 0 && maxCount > 0;

  const commit = useCallback(
    (nextValue: string) => {
      const nextCount = parseCount(nextValue);

      if (nextCount === null || nextCount < 1 || maxCount < 1) {
        return;
      }

      if (nextCount > maxCount) {
        setValue(committedValue);
        setHint(
          translate('SelectCountTooLargeInterp', {
            count: maxCount,
          })
        );

        return;
      }

      setHint(null);
      setCommittedValue(String(nextCount));
      selectDispatch({ type: 'selectFirstN', n: nextCount });
    },
    [committedValue, maxCount, selectDispatch]
  );

  const onInputChange = useCallback(
    (event: ChangeEvent<HTMLInputElement>) => {
      setValue(event.target.value);
      setHint(null);
    },
    [setValue, setHint]
  );

  const onInputBlur = useCallback(() => {
    commit(value);
  }, [commit, value]);

  const onInputKeyDown = useCallback(
    (event: KeyboardEvent<HTMLInputElement>) => {
      if (event.key === 'Enter') {
        event.preventDefault();
        commit(value);
      }
    },
    [commit, value]
  );

  const onGoPress = useCallback(() => {
    commit(value);
  }, [commit, value]);

  const onPresetPress = useCallback(
    (preset: number) => {
      const presetCount = Math.min(preset, maxCount);

      if (presetCount < 1) {
        return;
      }

      setValue(String(presetCount));
      setCommittedValue(String(presetCount));
      setHint(null);
      selectDispatch({ type: 'selectFirstN', n: presetCount });
    },
    [maxCount, selectDispatch]
  );

  const onSelectAllPress = useCallback(() => {
    setValue('');
    setCommittedValue('');
    setHint(null);
    selectDispatch({ type: 'selectAll' });
  }, [selectDispatch]);

  if (!isSelectMode) {
    return null;
  }

  return (
    <div className={styles.selectCount}>
      <input
        className={styles.input}
        type="number"
        inputMode="numeric"
        min={1}
        step={1}
        aria-label={translate('SelectCount')}
        title={translate('SelectCount')}
        value={value}
        onChange={onInputChange}
        onBlur={onInputBlur}
        onKeyDown={onInputKeyDown}
      />

      <Button
        className={styles.goButton}
        size={sizes.SMALL}
        isDisabled={!isCommittable}
        onPress={onGoPress}
      >
        {translate('Go')}
      </Button>

      <Menu>
        <Link
          className={styles.presetsButton}
          isDisabled={maxCount < 1}
          title={translate('SelectCountPresets')}
          aria-label={translate('SelectCountPresets')}
        >
          <Icon name={icons.CARET_DOWN} size={16} />
        </Link>

        <MenuContent>
          {PRESETS.map((preset) => {
            return (
              <PresetMenuItem
                key={preset}
                preset={preset}
                isDisabled={maxCount < 1}
                onPress={onPresetPress}
              />
            );
          })}

          <MenuItem isDisabled={maxCount < 1} onPress={onSelectAllPress}>
            {translate('All')}
          </MenuItem>
        </MenuContent>
      </Menu>

      {hint === null ? null : (
        <div className={styles.hint} role="alert">
          {hint}
        </div>
      )}
    </div>
  );
}

export default MovieIndexSelectCountButton;
