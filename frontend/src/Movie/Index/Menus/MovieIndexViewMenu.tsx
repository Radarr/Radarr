import React from 'react';
import MenuContent from 'Components/Menu/MenuContent';
import ViewMenu from 'Components/Menu/ViewMenu';
import ViewMenuItem from 'Components/Menu/ViewMenuItem';
import { align } from 'Helpers/Props';
import translate from 'Utilities/String/translate';

interface MovieIndexViewMenuProps {
  view: string;
  isDisabled: boolean;
  onViewSelect(value: string): unknown;
}

function MovieIndexViewMenu(props: MovieIndexViewMenuProps) {
  const { view, isDisabled, onViewSelect } = props;

  return (
    <ViewMenu isDisabled={isDisabled} alignMenu={align.RIGHT}>
      <MenuContent>
        <ViewMenuItem name="table" selectedView={view} onPress={onViewSelect}>
          {translate('Table')}
        </ViewMenuItem>

        <ViewMenuItem name="posters" selectedView={view} onPress={onViewSelect}>
          {translate('Posters')}
        </ViewMenuItem>

        <ViewMenuItem
          name="overview"
          selectedView={view}
          onPress={onViewSelect}
        >
          {translate('Overview')}
        </ViewMenuItem>
      </MenuContent>
    </ViewMenu>
  );
}

export default MovieIndexViewMenu;
