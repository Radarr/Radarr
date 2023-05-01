import React, { SyntheticEvent, useCallback } from 'react';
import { useSelect } from 'App/SelectContext';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import { icons } from 'Helpers/Props';
import styles from './MovieIndexPosterSelect.css';

interface MovieIndexPosterSelectProps {
  movieId: number;
}

function MovieIndexPosterSelect(props: MovieIndexPosterSelectProps) {
  const { movieId } = props;
  const [selectState, selectDispatch] = useSelect();
  const isSelected = selectState.selectedState[movieId];

  const onSelectPress = useCallback(
    (event: SyntheticEvent) => {
      const nativeEvent = event.nativeEvent as PointerEvent;
      const shiftKey = nativeEvent.shiftKey;

      selectDispatch({
        type: 'toggleSelected',
        id: movieId,
        isSelected: !isSelected,
        shiftKey,
      });
    },
    [movieId, isSelected, selectDispatch]
  );

  return (
    <Link className={styles.checkButton} onPress={onSelectPress}>
      <span className={styles.checkContainer}>
        <Icon
          className={isSelected ? styles.selected : styles.unselected}
          name={isSelected ? icons.CHECK_CIRCLE : icons.CIRCLE_OUTLINE}
          size={20}
        />
      </span>
    </Link>
  );
}

export default MovieIndexPosterSelect;
