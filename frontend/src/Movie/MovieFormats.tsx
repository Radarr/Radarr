import React from 'react';
import Label from 'Components/Label';
import { kinds } from 'Helpers/Props';
import CustomFormat from 'typings/CustomFormat';

interface MovieFormatsProps {
  formats: CustomFormat[];
}

function MovieFormats({ formats }: MovieFormatsProps) {
  return (
    <div>
      {formats.map(({ id, name }) => (
        <Label key={id} kind={kinds.INFO}>
          {name}
        </Label>
      ))}
    </div>
  );
}

export default MovieFormats;
