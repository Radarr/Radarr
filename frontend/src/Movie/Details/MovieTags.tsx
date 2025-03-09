import React from 'react';
import Label from 'Components/Label';
import { kinds, sizes } from 'Helpers/Props';
import useMovie from 'Movie/useMovie';
import useTags from 'Tags/useTags';
import sortByProp from 'Utilities/Array/sortByProp';

interface MovieTagsProps {
  movieId: number;
}

function MovieTags({ movieId }: MovieTagsProps) {
  const movie = useMovie(movieId)!;
  const tagList = useTags();

  const tags = movie.tags
    .map((tagId) => tagList.find((tag) => tag.id === tagId))
    .filter((tag) => !!tag)
    .sort(sortByProp('label'))
    .map((tag) => tag.label);

  return (
    <div>
      {tags.map((tag) => {
        return (
          <Label key={tag} kind={kinds.INFO} size={sizes.LARGE}>
            {tag}
          </Label>
        );
      })}
    </div>
  );
}

export default MovieTags;
