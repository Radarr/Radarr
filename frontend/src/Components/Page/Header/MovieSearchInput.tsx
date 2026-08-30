import { push } from 'connected-react-router';
import { ExtendedKeyboardEvent } from 'mousetrap';
import React, {
  FormEvent,
  KeyboardEvent,
  SyntheticEvent,
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
} from 'react';
import Autosuggest from 'react-autosuggest';
import { useDispatch } from 'react-redux';
import { useDebouncedCallback } from 'use-debounce';
import { Tag } from 'App/State/TagsAppState';
import Icon from 'Components/Icon';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import useKeyboardShortcuts from 'Helpers/Hooks/useKeyboardShortcuts';
import { icons } from 'Helpers/Props';
import Movie from 'Movie/Movie';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import translate from 'Utilities/String/translate';
import MovieSearchResult from './MovieSearchResult';
import styles from './MovieSearchInput.css';

const ADD_NEW_TYPE = 'addNew';

interface Match {
  key: string;
  refIndex: number;
}

interface AddNewMovieSuggestion {
  type: 'addNew';
  title: string;
}

export interface SuggestedMovie
  extends Pick<
    Movie,
    | 'title'
    | 'originalTitle'
    | 'year'
    | 'titleSlug'
    | 'sortTitle'
    | 'images'
    | 'alternateTitles'
    | 'tmdbId'
    | 'imdbId'
  > {
  firstCharacter: string;
  tags: Tag[];
}

interface MovieSuggestion {
  title: string;
  indices: number[];
  item: SuggestedMovie;
  matches: Match[];
  refIndex: number;
}

interface Section {
  title: string;
  loading?: boolean;
  suggestions: MovieSuggestion[] | AddNewMovieSuggestion[];
}

function MovieSearchInput() {
  const dispatch = useDispatch();
  const { bindShortcut, unbindShortcut } = useKeyboardShortcuts();

  const [value, setValue] = useState('');
  const [requestLoading, setRequestLoading] = useState(false);
  const [suggestions, setSuggestions] = useState<MovieSuggestion[]>([]);

  const autosuggestRef = useRef<Autosuggest>(null);
  const inputRef = useRef<HTMLInputElement>(null);
  const abortRequest = useRef<(() => void) | null>(null);
  const isLoading = useRef(false);
  const requestValue = useRef<string | null>(null);

  const suggestionGroups = useMemo(() => {
    const result: Section[] = [];

    if (suggestions.length || requestLoading) {
      result.push({
        title: translate('ExistingMovies'),
        loading: requestLoading,
        suggestions,
      });
    }

    result.push({
      title: translate('AddNewMovie'),
      suggestions: [
        {
          type: ADD_NEW_TYPE,
          title: value,
        },
      ],
    });

    return result;
  }, [requestLoading, suggestions, value]);

  const requestSuggestions = useDebouncedCallback((value: string) => {
    if (!isLoading.current) {
      return;
    }

    requestValue.current = value;
    setRequestLoading(true);

    abortRequest.current?.();

    const ajaxRequest = createAjaxRequest({
      url: '/movie/search',
      data: { term: value, limit: 10 },
    });

    abortRequest.current = ajaxRequest.abortRequest;
    ajaxRequest.request.done((movies: Movie[]) => {
      if (value !== requestValue.current) {
        return;
      }

      setSuggestions(
        movies.map((movie) => ({
          title: movie.title,
          item: {
            ...movie,
            firstCharacter: movie.title.charAt(0).toLowerCase(),
            tags: [],
          },
          indices: [],
          matches: [{ key: 'title', refIndex: 0 }],
          refIndex: 0,
        }))
      );
      requestValue.current = null;
      setRequestLoading(false);
      isLoading.current = false;
    });

    ajaxRequest.request.fail(() => {
      if (value === requestValue.current) {
        setSuggestions([]);
        requestValue.current = null;
        setRequestLoading(false);
        isLoading.current = false;
      }
    });
  }, 250);

  const reset = useCallback(() => {
    setValue('');
    setSuggestions([]);
    // setLoading(false);
    isLoading.current = false;
  }, []);

  const focusInput = useCallback((event: ExtendedKeyboardEvent) => {
    event.preventDefault();
    inputRef.current?.focus();
  }, []);

  const getSectionSuggestions = useCallback((section: Section) => {
    return section.suggestions;
  }, []);

  const renderSectionTitle = useCallback((section: Section) => {
    return (
      <div className={styles.sectionTitle}>
        {section.title}

        {section.loading && (
          <LoadingIndicator
            className={styles.loading}
            rippleClassName={styles.ripple}
            size={20}
          />
        )}
      </div>
    );
  }, []);

  const getSuggestionValue = useCallback(({ title }: { title: string }) => {
    return title;
  }, []);

  const renderSuggestion = useCallback(
    (
      item: AddNewMovieSuggestion | MovieSuggestion,
      { query }: { query: string }
    ) => {
      if ('type' in item) {
        return (
          <div className={styles.addNewMovieSuggestion}>
            {translate('SearchForQuery', { query })}
          </div>
        );
      }

      return <MovieSearchResult {...item.item} match={item.matches[0]} />;
    },
    []
  );

  const handleChange = useCallback(
    (
      _event: FormEvent<HTMLElement>,
      {
        newValue,
        method,
      }: {
        newValue: string;
        method: 'down' | 'up' | 'escape' | 'enter' | 'click' | 'type';
      }
    ) => {
      if (method === 'up' || method === 'down') {
        return;
      }

      setValue(newValue);
    },
    []
  );

  const handleKeyDown = useCallback(
    (event: KeyboardEvent<HTMLElement>) => {
      if (event.shiftKey || event.altKey || event.ctrlKey) {
        return;
      }

      if (event.key === 'Escape') {
        reset();
        return;
      }

      if (event.key !== 'Tab' && event.key !== 'Enter') {
        return;
      }

      if (!autosuggestRef.current) {
        return;
      }

      const { highlightedSectionIndex, highlightedSuggestionIndex } =
        autosuggestRef.current.state;

      if (!suggestions.length || highlightedSectionIndex) {
        dispatch(
          push(
            `${window.Radarr.urlBase}/add/new?term=${encodeURIComponent(value)}`
          )
        );

        inputRef.current?.blur();
        reset();

        return;
      }

      // If a suggestion is not selected go to the first movie,
      // otherwise go to the selected movie.

      const selectedSuggestion =
        highlightedSuggestionIndex == null
          ? suggestions[0]
          : suggestions[highlightedSuggestionIndex];

      dispatch(
        push(
          `${window.Radarr.urlBase}/movie/${selectedSuggestion.item.titleSlug}`
        )
      );

      inputRef.current?.blur();
      reset();
    },
    [value, suggestions, dispatch, reset]
  );

  const handleBlur = useCallback(() => {
    reset();
  }, [reset]);

  const handleSuggestionsFetchRequested = useCallback(
    ({ value }: { value: string }) => {
      isLoading.current = true;

      requestSuggestions(value);
    },
    [requestSuggestions]
  );

  const handleSuggestionsClearRequested = useCallback(() => {
    setSuggestions([]);
    isLoading.current = false;
  }, []);

  const handleSuggestionSelected = useCallback(
    (
      _event: SyntheticEvent,
      { suggestion }: { suggestion: MovieSuggestion | AddNewMovieSuggestion }
    ) => {
      if ('type' in suggestion) {
        dispatch(
          push(
            `${window.Radarr.urlBase}/add/new?term=${encodeURIComponent(value)}`
          )
        );
      } else {
        setValue('');
        dispatch(
          push(`${window.Radarr.urlBase}/movie/${suggestion.item.titleSlug}`)
        );
      }
    },
    [value, dispatch]
  );

  const inputProps = {
    ref: inputRef,
    className: styles.input,
    name: 'movieSearch',
    value,
    placeholder: translate('Search'),
    autoComplete: 'off',
    spellCheck: false,
    onChange: handleChange,
    onKeyDown: handleKeyDown,
    onBlur: handleBlur,
  };

  const theme = {
    container: styles.container,
    containerOpen: styles.containerOpen,
    suggestionsContainer: styles.movieContainer,
    suggestionsList: styles.list,
    suggestion: styles.listItem,
    suggestionHighlighted: styles.highlighted,
  };

  useEffect(() => () => abortRequest.current?.(), []);

  useEffect(() => {
    bindShortcut('focusMovieSearchInput', focusInput);

    return () => {
      unbindShortcut('focusMovieSearchInput');
    };
  }, [bindShortcut, unbindShortcut, focusInput]);

  return (
    <div className={styles.wrapper}>
      <Icon name={icons.SEARCH} />

      <Autosuggest
        ref={autosuggestRef}
        inputProps={inputProps}
        theme={theme}
        focusInputOnSuggestionClick={false}
        multiSection={true}
        suggestions={suggestionGroups}
        getSectionSuggestions={getSectionSuggestions}
        renderSectionTitle={renderSectionTitle}
        getSuggestionValue={getSuggestionValue}
        renderSuggestion={renderSuggestion}
        onSuggestionSelected={handleSuggestionSelected}
        onSuggestionsFetchRequested={handleSuggestionsFetchRequested}
        onSuggestionsClearRequested={handleSuggestionsClearRequested}
      />
    </div>
  );
}

export default MovieSearchInput;
