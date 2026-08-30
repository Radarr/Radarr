import { throttle } from 'lodash';
import React, {
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
} from 'react';
import { useSelector } from 'react-redux';
import { FixedSizeList as List, ListChildComponentProps } from 'react-window';
import { useDebouncedCallback } from 'use-debounce';
import TextInput from 'Components/Form/TextInput';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import Scroller from 'Components/Scroller/Scroller';
import Column from 'Components/Table/Column';
import VirtualTableRowButton from 'Components/Table/VirtualTableRowButton';
import { scrollDirections } from 'Helpers/Props';
import Movie from 'Movie/Movie';
import createAllMoviesSelector from 'Store/Selectors/createAllMoviesSelector';
import dimensions from 'Styles/Variables/dimensions';
import { InputChanged } from 'typings/inputs';
import sortByProp from 'Utilities/Array/sortByProp';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import translate from 'Utilities/String/translate';
import SelectMovieModalTableHeader from './SelectMovieModalTableHeader';
import SelectMovieRow from './SelectMovieRow';
import styles from './SelectMovieModalContent.css';

const columns = [
  {
    name: 'title',
    label: () => translate('Title'),
    isVisible: true,
  },
  {
    name: 'year',
    label: () => translate('Year'),
    isVisible: true,
  },
  {
    name: 'imdbId',
    label: () => translate('IMDbId'),
    isVisible: true,
  },
  {
    name: 'tmdbId',
    label: () => translate('TMDBId'),
    isVisible: true,
  },
];

const bodyPadding = parseInt(dimensions.pageContentBodyPadding);

interface SelectMovieModalContentProps {
  modalTitle: string;
  onMovieSelect(movie: Movie): void;
  onModalClose(): void;
}

interface RowItemData {
  items: Movie[];
  columns: Column[];
  onMovieSelect(movieId: number): void;
}

function Row({ index, style, data }: ListChildComponentProps<RowItemData>) {
  const { items, onMovieSelect } = data;
  const movie = index >= items.length ? null : items[index];

  const handlePress = useCallback(() => {
    if (movie?.id) {
      onMovieSelect(movie.id);
    }
  }, [movie?.id, onMovieSelect]);

  if (movie == null) {
    return null;
  }

  return (
    <VirtualTableRowButton
      style={{
        display: 'flex',
        justifyContent: 'space-between',
        ...style,
      }}
      onPress={handlePress}
    >
      <SelectMovieRow
        key={movie.id}
        title={movie.title}
        tmdbId={movie.tmdbId}
        imdbId={movie.imdbId}
        year={movie.year}
      />
    </VirtualTableRowButton>
  );
}

function SelectMovieModalContent(props: SelectMovieModalContentProps) {
  const { modalTitle, onMovieSelect, onModalClose } = props;

  const listRef = useRef<List<RowItemData>>(null);
  const scrollerRef = useRef<HTMLDivElement>(null);
  const allMovies: Movie[] = useSelector(createAllMoviesSelector());
  const [remoteMovies, setRemoteMovies] = useState<Movie[]>([]);
  const abortRequest = useRef<(() => void) | null>(null);
  const [filter, setFilter] = useState('');
  const [size, setSize] = useState({ width: 0, height: 0 });
  const windowHeight = window.innerHeight;

  useEffect(() => {
    const current = scrollerRef?.current as HTMLElement;

    if (current) {
      const width = current.clientWidth;
      const height = current.clientHeight;
      const padding = bodyPadding - 5;

      setSize({
        width: width - padding * 2,
        height: height + padding,
      });
    }
  }, [windowHeight, scrollerRef]);

  useEffect(() => {
    const currentScrollerRef = scrollerRef.current as HTMLElement;
    const currentScrollListener = currentScrollerRef;

    const handleScroll = throttle(() => {
      const { offsetTop = 0 } = currentScrollerRef;
      const scrollTop = currentScrollerRef.scrollTop - offsetTop;

      listRef.current?.scrollTo(scrollTop);
    }, 10);

    currentScrollListener.addEventListener('scroll', handleScroll);

    return () => {
      handleScroll.cancel();

      if (currentScrollListener) {
        currentScrollListener.removeEventListener('scroll', handleScroll);
      }
    };
  }, [listRef, scrollerRef]);

  const searchMovies = useDebouncedCallback((term: string) => {
    abortRequest.current?.();

    const ajaxRequest = createAjaxRequest({
      url: term.trim() ? '/movie/search' : '/movie/page',
      data: term.trim()
        ? { term, limit: 20 }
        : { page: 1, pageSize: 100, sortKey: 'sortTitle' },
    });

    abortRequest.current = ajaxRequest.abortRequest;
    ajaxRequest.request.done((response: Movie[] | { records: Movie[] }) => {
      setRemoteMovies(Array.isArray(response) ? response : response.records);
    });
  }, 250);

  useEffect(() => {
    searchMovies('');

    return () => {
      abortRequest.current?.();
      searchMovies.cancel();
    };
  }, [searchMovies]);

  const onFilterChange = useCallback(
    ({ value }: InputChanged<string>) => {
      setFilter(value);
      searchMovies(value);
    },
    [searchMovies]
  );

  const sortedMovies = useMemo(() => {
    const movies = new Map<number, Movie>();

    [...allMovies, ...remoteMovies].forEach((movie) => {
      movies.set(movie.id, movie);
    });

    return Array.from(movies.values()).sort(sortByProp('sortTitle'));
  }, [allMovies, remoteMovies]);

  const onMovieSelectWrapper = useCallback(
    (movieId: number) => {
      const movie = sortedMovies.find((item) => item.id === movieId) as Movie;

      onMovieSelect(movie);
    },
    [sortedMovies, onMovieSelect]
  );

  const items = useMemo(
    () =>
      sortedMovies.filter(
        (item) =>
          item.title.toLowerCase().includes(filter.toLowerCase()) ||
          item.tmdbId.toString().includes(filter) ||
          item.imdbId?.includes(filter)
      ),
    [sortedMovies, filter]
  );

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {translate('SelectMovieModalTitle', { modalTitle })}
      </ModalHeader>

      <ModalBody
        className={styles.modalBody}
        scrollDirection={scrollDirections.NONE}
      >
        <TextInput
          className={styles.filterInput}
          placeholder={translate('FilterMoviePlaceholder')}
          name="filter"
          value={filter}
          autoFocus={true}
          onChange={onFilterChange}
        />

        <Scroller
          ref={scrollerRef}
          className={styles.scroller}
          autoFocus={false}
        >
          <SelectMovieModalTableHeader columns={columns} />
          <List<RowItemData>
            ref={listRef}
            style={{
              width: '100%',
              height: '100%',
              overflow: 'none',
            }}
            width={size.width}
            height={size.height}
            itemCount={items.length}
            itemSize={38}
            itemData={{
              items,
              columns,
              onMovieSelect: onMovieSelectWrapper,
            }}
          >
            {Row}
          </List>
        </Scroller>
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Cancel')}</Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default SelectMovieModalContent;
