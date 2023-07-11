import React from 'react';
import Icon from 'Components/Icon';
import Label from 'Components/Label';
import { icons, kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './MoviePopularityIndex.css';

interface MoviePopularityIndexProps {
  popularity: number;
}

function MoviePopularityIndex(props: MoviePopularityIndexProps) {
  const { popularity } = props;

  return (
    <Label kind={kinds.INVERSE} title={translate('PopularityIndex')}>
      <Icon className={styles.popularityIcon} name={icons.POPULAR} size={11} />
      {popularity.toFixed()}
    </Label>
  );
}

export default MoviePopularityIndex;
