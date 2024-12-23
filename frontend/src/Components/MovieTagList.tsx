import React from 'react';
import { useSelector } from 'react-redux';
import createTagsSelector from 'Store/Selectors/createTagsSelector';
import TagList from './TagList';

interface MovieTagListProps {
  tags: number[];
}

function MovieTagList({ tags }: MovieTagListProps) {
  const tagList = useSelector(createTagsSelector());

  return <TagList tags={tags} tagList={tagList} />;
}

export default MovieTagList;
