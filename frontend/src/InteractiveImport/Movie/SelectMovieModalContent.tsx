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
import TextInput from 'Components/Form/TextInput';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import Scroller from 'Components/Scroller/Scroller';
import { scrollDirections } from 'Helpers/Props';
import Movie from 'Movie/Movie';
import createAllMoviesSelector from 'Store/Selectors/createAllMoviesSelector';
import dimensions from 'Styles/Variables/dimensions';
import SelectMovieRow from './SelectMovieRow';
import styles from './SelectMovieModalContent.css';

const bodyPadding = parseInt(dimensions.pageContentBodyPadding);

interface SelectMovieModalContentProps {
  modalTitle: string;
  onMovieSelect(movie: Movie): void;
  onModalClose(): void;
}

interface RowItemData {
  items: Movie[];
  onMovieSelect(movieId: number): void;
}

const Row: React.FC<ListChildComponentProps<RowItemData>> = ({
  index,
  style,
  data,
}) => {
  const { items, onMovieSelect } = data;

  if (index >= items.length) {
    return null;
  }

  const movie = items[index];

  return (
    <div
      style={{
        display: 'flex',
        justifyContent: 'space-between',
        ...style,
      }}
    >
      <SelectMovieRow
        id={movie.id}
        title={movie.title}
        year={movie.year}
        onMovieSelect={onMovieSelect}
      />
    </div>
  );
};

function SelectMovieModalContent(props: SelectMovieModalContentProps) {
  const { modalTitle, onMovieSelect, onModalClose } = props;

  const listRef = useRef<List<RowItemData>>(null);
  const scrollerRef = useRef<HTMLDivElement>(null);
  const allMovies: Movie[] = useSelector(createAllMoviesSelector());
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

  const onFilterChange = useCallback(
    ({ value }: { value: string }) => {
      setFilter(value);
    },
    [setFilter]
  );

  const onMovieSelectWrapper = useCallback(
    (movieId: number) => {
      const movie = allMovies.find((s) => s.id === movieId) as Movie;

      onMovieSelect(movie);
    },
    [allMovies, onMovieSelect]
  );

  const items = useMemo(() => {
    const sorted = [...allMovies].sort((a, b) =>
      a.sortTitle.localeCompare(b.sortTitle)
    );

    return sorted.filter((item) =>
      item.title.toLowerCase().includes(filter.toLowerCase())
    );
  }, [allMovies, filter]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{modalTitle} - Select Movie</ModalHeader>

      <ModalBody
        className={styles.modalBody}
        scrollDirection={scrollDirections.NONE}
      >
        <TextInput
          className={styles.filterInput}
          placeholder="Filter movies"
          name="filter"
          value={filter}
          autoFocus={true}
          onChange={onFilterChange}
        />

        <Scroller
          className={styles.scroller}
          autoFocus={false}
          ref={scrollerRef}
        >
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
              onMovieSelect: onMovieSelectWrapper,
            }}
          >
            {Row}
          </List>
        </Scroller>
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>Cancel</Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default SelectMovieModalContent;
