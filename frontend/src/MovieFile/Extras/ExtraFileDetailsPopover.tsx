import React from 'react';
import IconButton from 'Components/Link/IconButton';
import Popover from 'Components/Tooltip/Popover';
import { icons, tooltipPositions } from 'Helpers/Props';
import { ExtraFileType } from 'MovieFile/ExtraFile';
import translate from 'Utilities/String/translate';

interface ExtraFileDetailsPopoverProps {
  type: ExtraFileType;
  title?: string;
  languageTags?: string[];
}

function ExtraFileDetailsPopover(props: ExtraFileDetailsPopoverProps) {
  const { type, title, languageTags = [] } = props;

  const details = [];

  if (type === 'subtitle') {
    if (title) {
      details.push({ name: translate('Title'), value: title });
    }

    if (languageTags.length) {
      details.push({
        name: translate('Disposition'),
        value: languageTags.join(', '),
      });
    }
  }

  if (details.length) {
    return (
      <Popover
        anchor={<IconButton name={icons.INFO} />}
        title={translate('Tags')}
        body={
          <ul>
            {details.map(({ name, value }, index) => {
              return (
                <li key={index}>
                  {name}: {value}
                </li>
              );
            })}
          </ul>
        }
        position={tooltipPositions.LEFT}
      />
    );
  }

  return '';
}

export default ExtraFileDetailsPopover;
