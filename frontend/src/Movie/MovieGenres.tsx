import React from 'react';
import Label from 'Components/Label';
import Tooltip from 'Components/Tooltip/Tooltip';
import { kinds, sizes, tooltipPositions } from 'Helpers/Props';

interface MovieGenresProps {
  className?: string;
  genres: string[];
}

function MovieGenres({ className, genres }: MovieGenresProps) {
  const firstGenres = genres.slice(0, 3);
  const otherGenres = genres.slice(3);

  if (otherGenres.length) {
    return (
      <Tooltip
        anchor={<span className={className}>{firstGenres.join(', ')}</span>}
        tooltip={
          <div>
            {otherGenres.map((tag) => {
              return (
                <Label key={tag} kind={kinds.INFO} size={sizes.LARGE}>
                  {tag}
                </Label>
              );
            })}
          </div>
        }
        kind={kinds.INVERSE}
        position={tooltipPositions.TOP}
      />
    );
  }

  return <span className={className}>{firstGenres.join(', ')}</span>;
}

export default MovieGenres;
