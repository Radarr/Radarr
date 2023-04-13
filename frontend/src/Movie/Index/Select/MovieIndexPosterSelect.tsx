import React, { useCallback } from 'react';
import { SelectActionType, useSelect } from 'App/SelectContext';
import IconButton from 'Components/Link/IconButton';
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
    (event) => {
      const shiftKey = event.nativeEvent.shiftKey;

      selectDispatch({
        type: SelectActionType.ToggleSelected,
        id: movieId,
        isSelected: !isSelected,
        shiftKey,
      });
    },
    [movieId, isSelected, selectDispatch]
  );

  return (
    <IconButton
      className={styles.checkContainer}
      iconClassName={isSelected ? styles.selected : styles.unselected}
      name={isSelected ? icons.CHECK_CIRCLE : icons.CIRCLE_OUTLINE}
      size={20}
      onPress={onSelectPress}
    />
  );
}

export default MovieIndexPosterSelect;
