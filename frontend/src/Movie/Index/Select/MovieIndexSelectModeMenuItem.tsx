import { IconDefinition } from '@fortawesome/fontawesome-common-types';
import React, { useCallback } from 'react';
import { useSelect } from 'App/SelectContext';
import PageToolbarOverflowMenuItem from 'Components/Page/Toolbar/PageToolbarOverflowMenuItem';

interface MovieIndexSelectModeMenuItemProps {
  label: string;
  iconName: IconDefinition;
  isSelectMode: boolean;
  onPress: () => void;
}

function MovieIndexSelectModeMenuItem(
  props: MovieIndexSelectModeMenuItemProps
) {
  const { label, iconName, isSelectMode, onPress } = props;
  const [, selectDispatch] = useSelect();

  const onPressWrapper = useCallback(() => {
    if (isSelectMode) {
      selectDispatch({
        type: 'reset',
      });
    }

    onPress();
  }, [isSelectMode, onPress, selectDispatch]);

  return (
    <PageToolbarOverflowMenuItem
      label={label}
      iconName={iconName}
      onPress={onPressWrapper}
    />
  );
}

export default MovieIndexSelectModeMenuItem;
