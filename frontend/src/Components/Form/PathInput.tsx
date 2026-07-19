import classNames from 'classnames';
import React, {
  KeyboardEvent,
  SyntheticEvent,
  useCallback,
  useEffect,
  useState,
} from 'react';
import {
  ChangeEvent,
  SuggestionsFetchRequestedParams,
} from 'react-autosuggest';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import { useDebouncedCallback } from 'use-debounce';
import AppState from 'App/State/AppState';
import { Path } from 'App/State/PathsAppState';
import FileBrowserModal from 'Components/FileBrowser/FileBrowserModal';
import Icon from 'Components/Icon';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { icons } from 'Helpers/Props';
import { clearPaths, fetchPaths } from 'Store/Actions/pathActions';
import { InputChanged } from 'typings/inputs';
import AutoSuggestInput from './AutoSuggestInput';
import FormInputButton from './FormInputButton';
import styles from './PathInput.css';

export interface PathInputProps {
  className?: string;
  name: string;
  value?: string;
  placeholder?: string;
  includeFiles: boolean;
  hasButton?: boolean;
  hasFileBrowser?: boolean;
  onChange: (change: InputChanged<string>) => void;
}

interface PathInputInternalProps extends PathInputProps {
  paths: Path[];
  onFetchPaths: (path: string) => void;
  onClearPaths: () => void;
}

function handleSuggestionsClearRequested() {
  // Required because props aren't always rendered, but no-op
  // because we don't want to reset the paths after a path is selected.
}

function createPathsSelector() {
  return createSelector(
    (state: AppState) => state.paths,
    (paths) => {
      const { currentPath, directories, files } = paths;

      const filteredPaths = [...directories, ...files].filter(({ path }) => {
        return path.toLowerCase().startsWith(currentPath.toLowerCase());
      });

      return filteredPaths;
    }
  );
}

function PathInput(props: PathInputProps) {
  const { includeFiles } = props;

  const dispatch = useDispatch();

  const paths = useSelector(createPathsSelector());

  const handleFetchPaths = useCallback(
    (path: string) => {
      dispatch(fetchPaths({ path, includeFiles }));
    },
    [includeFiles, dispatch]
  );

  const handleClearPaths = useCallback(() => {
    dispatch(clearPaths);
  }, [dispatch]);

  return (
    <PathInputInternal
      {...props}
      paths={paths}
      onFetchPaths={handleFetchPaths}
      onClearPaths={handleClearPaths}
    />
  );
}

export default PathInput;

export function PathInputInternal(props: PathInputInternalProps) {
  const {
    className = styles.inputWrapper,
    name,
    value: inputValue = '',
    paths,
    includeFiles,
    hasButton,
    hasFileBrowser = true,
    onChange,
    onFetchPaths,
    onClearPaths,
    ...otherProps
  } = props;

  const [value, setValue] = useState(inputValue);
  const [isFileBrowserModalOpen, setIsFileBrowserModalOpen] = useState(false);
  const previousInputValue = usePrevious(inputValue);
  const dispatch = useDispatch();

  const handleFetchPaths = useCallback(
    (path: string) => {
      dispatch(fetchPaths({ path, includeFiles }));
    },
    [includeFiles, dispatch]
  );

  // Typing resolves every keystroke server-side; debounce so a pause
  // fetches once instead of once per character.
  const debouncedFetchPaths = useDebouncedCallback(handleFetchPaths, 150);

  const handleInputChange = useCallback(
    (_event: SyntheticEvent, { newValue }: ChangeEvent) => {
      setValue(newValue);
    },
    [setValue]
  );

  // Match each typed segment against the candidate's segment at the same
  // depth, so any contiguous part of any directory name matches its level
  // (`/down/dbd` matches `/downloads/[DBD-Raws].../`).
  const searchSegments = value
    .split(/[\\/]/)
    .filter((segment) => segment.length)
    .map((segment) => segment.toLowerCase());

  const filteredPaths = searchSegments.length
    ? paths.filter(({ path: candidatePath }) => {
        const candidateSegments = candidatePath
          .split(/[\\/]/)
          .filter((segment) => segment.length);

        return searchSegments.every((segment, index) =>
          candidateSegments[index]?.toLowerCase().includes(segment)
        );
      })
    : paths;

  const handleInputKeyDown = useCallback(
    (event: KeyboardEvent<HTMLElement>) => {
      if (event.key !== 'Tab') {
        return;
      }

      const path = filteredPaths[0];

      // Only capture Tab when it would complete a different path;
      // otherwise let it move focus to the next field.
      if (path && path.path !== value) {
        event.preventDefault();

        onChange({
          name,
          value: path.path,
        });

        if (path.type !== 'file') {
          handleFetchPaths(path.path);
        }
      }
    },
    [name, value, filteredPaths, handleFetchPaths, onChange]
  );
  const handleInputBlur = useCallback(() => {
    onChange({
      name,
      value,
    });

    onClearPaths();
  }, [name, value, onClearPaths, onChange]);

  const handleSuggestionSelected = useCallback(
    (_event: SyntheticEvent, { suggestion }: { suggestion: Path }) => {
      handleFetchPaths(suggestion.path);
    },
    [handleFetchPaths]
  );

  const handleSuggestionsFetchRequested = useCallback(
    ({ value: newValue }: SuggestionsFetchRequestedParams) => {
      debouncedFetchPaths(newValue);
    },
    [debouncedFetchPaths]
  );

  const handleFileBrowserOpenPress = useCallback(() => {
    setIsFileBrowserModalOpen(true);
  }, [setIsFileBrowserModalOpen]);

  const handleFileBrowserModalClose = useCallback(() => {
    setIsFileBrowserModalOpen(false);
  }, [setIsFileBrowserModalOpen]);

  const handleChange = useCallback(
    (change: InputChanged<Path>) => {
      onChange({ name, value: change.value.path });
    },
    [name, onChange]
  );

  const getSuggestionValue = useCallback(({ path }: Path) => path, []);

  const renderSuggestion = useCallback(
    ({ path }: Path, { query }: { query: string }) => {
      // Strip one trailing separator so `/downloads/Kung/` still
      // highlights `Kung` (the segment being searched).
      const trimmedQuery = /[\\/]$/.test(query) ? query.slice(0, -1) : query;
      const searchValue = trimmedQuery
        .slice(
          Math.max(
            trimmedQuery.lastIndexOf('/'),
            trimmedQuery.lastIndexOf('\\')
          ) + 1
        )
        .toLowerCase();

      // Highlight the match within the entry name (after its parent
      // separator), not the shared parent portion of the path.
      const parentEnd =
        path.endsWith('/') || path.endsWith('\\')
          ? path.length - 1
          : path.length;
      const nameStart =
        Math.max(
          path.lastIndexOf('/', parentEnd - 1),
          path.lastIndexOf('\\', parentEnd - 1)
        ) + 1;

      const matchIndex = searchValue
        ? path.toLowerCase().indexOf(searchValue, nameStart)
        : -1;

      if (matchIndex === -1) {
        return <span>{path}</span>;
      }

      return (
        <span>
          {path.substring(0, matchIndex)}
          <span className={styles.pathMatch}>
            {path.substring(matchIndex, matchIndex + searchValue.length)}
          </span>
          {path.substring(matchIndex + searchValue.length)}
        </span>
      );
    },
    []
  );

  useEffect(() => {
    if (inputValue !== previousInputValue) {
      setValue(inputValue);
    }
  }, [inputValue, previousInputValue, setValue]);

  return (
    <div className={className}>
      <AutoSuggestInput
        {...otherProps}
        className={hasFileBrowser ? styles.hasFileBrowser : undefined}
        name={name}
        value={value}
        suggestions={filteredPaths}
        getSuggestionValue={getSuggestionValue}
        renderSuggestion={renderSuggestion}
        onInputKeyDown={handleInputKeyDown}
        onInputChange={handleInputChange}
        onInputBlur={handleInputBlur}
        onSuggestionSelected={handleSuggestionSelected}
        onSuggestionsFetchRequested={handleSuggestionsFetchRequested}
        onSuggestionsClearRequested={handleSuggestionsClearRequested}
        onChange={handleChange}
      />

      {hasFileBrowser ? (
        <>
          <FormInputButton
            className={classNames(
              styles.fileBrowserButton,
              hasButton && styles.fileBrowserMiddleButton
            )}
            onPress={handleFileBrowserOpenPress}
          >
            <Icon name={icons.FOLDER_OPEN} />
          </FormInputButton>

          <FileBrowserModal
            isOpen={isFileBrowserModalOpen}
            name={name}
            value={value}
            includeFiles={includeFiles}
            onChange={onChange}
            onModalClose={handleFileBrowserModalClose}
          />
        </>
      ) : null}
    </div>
  );
}
