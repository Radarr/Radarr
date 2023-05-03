import React, { useCallback, useMemo, useState } from 'react';
import { useSelector } from 'react-redux';
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
import SelectMovieRow from './SelectMovieRow';
import styles from './SelectMovieModalContent.css';

interface SelectMovieModalContentProps {
  modalTitle: string;
  onMovieSelect(movie: Movie): void;
  onModalClose(): void;
}

function SelectMovieModalContent(props: SelectMovieModalContentProps) {
  const { modalTitle, onMovieSelect, onModalClose } = props;

  const allMovies: Movie[] = useSelector(createAllMoviesSelector());
  const [filter, setFilter] = useState('');

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

        <Scroller className={styles.scroller} autoFocus={false}>
          {items.map((item) => {
            return (
              <SelectMovieRow
                key={item.id}
                id={item.id}
                title={item.title}
                year={item.year}
                onMovieSelect={onMovieSelectWrapper}
              />
            );
          })}
        </Scroller>
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>Cancel</Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default SelectMovieModalContent;
