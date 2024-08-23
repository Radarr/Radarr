import React, { useCallback } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { MOVIE_SEARCH } from 'Commands/commandNames';
import IconButton from 'Components/Link/IconButton';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import useModalOpenState from 'Helpers/Hooks/useModalOpenState';
import { icons } from 'Helpers/Props';
import { executeCommand } from 'Store/Actions/commandActions';
import createExecutingCommandsSelector from 'Store/Selectors/createExecutingCommandsSelector';
import translate from 'Utilities/String/translate';
import MovieInteractiveSearchModal from './Search/MovieInteractiveSearchModal';
import styles from './MovieSearchCell.css';

interface MovieSearchCellProps {
  movieId: number;
  movieTitle: string;
}

function MovieSearchCell(props: MovieSearchCellProps) {
  const { movieId, movieTitle } = props;

  const executingCommands = useSelector(createExecutingCommandsSelector());
  const isSearching = executingCommands.some(({ name, body }) => {
    const { movieIds = [] } = body;
    return name === MOVIE_SEARCH && movieIds.indexOf(movieId) > -1;
  });

  const dispatch = useDispatch();

  const [
    isInteractiveSearchModalOpen,
    setInteractiveSearchModalOpen,
    setInteractiveSearchModalClosed,
  ] = useModalOpenState(false);

  const handleSearchPress = useCallback(() => {
    dispatch(
      executeCommand({
        name: MOVIE_SEARCH,
        movieIds: [movieId],
      })
    );
  }, [movieId, dispatch]);

  return (
    <TableRowCell className={styles.movieSearchCell}>
      <SpinnerIconButton
        name={icons.SEARCH}
        isSpinning={isSearching}
        title={translate('AutomaticSearch')}
        onPress={handleSearchPress}
      />

      <IconButton
        name={icons.INTERACTIVE}
        title={translate('InteractiveSearch')}
        onPress={setInteractiveSearchModalOpen}
      />

      <MovieInteractiveSearchModal
        isOpen={isInteractiveSearchModalOpen}
        movieId={movieId}
        movieTitle={movieTitle}
        onModalClose={setInteractiveSearchModalClosed}
      />
    </TableRowCell>
  );
}

export default MovieSearchCell;
