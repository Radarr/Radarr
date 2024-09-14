import React, { useCallback } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import {
  selectImportListSchema,
  setImportListFieldValue,
  setImportListValue,
} from 'Store/Actions/settingsActions';
import createMovieCreditImportListSelector from 'Store/Selectors/createMovieCreditImportListSelector';
import { MovieCastPosterProps } from './Cast/MovieCastPoster';
import { MovieCrewPosterProps } from './Crew/MovieCrewPoster';

type MovieCreditPosterProps = {
  component: React.ElementType;
} & (
  | Omit<MovieCrewPosterProps, 'onImportListSelect'>
  | Omit<MovieCastPosterProps, 'onImportListSelect'>
);

function MovieCreditPoster({
  component: ItemComponent,
  tmdbId,
  personName,
  ...otherProps
}: MovieCreditPosterProps) {
  const importList = useSelector(createMovieCreditImportListSelector(tmdbId));

  const dispatch = useDispatch();

  const handleImportListSelect = useCallback(() => {
    dispatch(
      selectImportListSchema({
        implementation: 'TMDbPersonImport',
        implementationName: 'TMDb Person',
        presetName: undefined,
      })
    );

    dispatch(
      // @ts-expect-error 'setImportListFieldValue' isn't typed yet
      setImportListFieldValue({ name: 'personId', value: tmdbId.toString() })
    );

    dispatch(
      // @ts-expect-error 'setImportListValue' isn't typed yet
      setImportListValue({ name: 'name', value: `${personName} - ${tmdbId}` })
    );
  }, [dispatch, tmdbId, personName]);

  return (
    <ItemComponent
      {...otherProps}
      tmdbId={tmdbId}
      personName={personName}
      importList={importList}
      onImportListSelect={handleImportListSelect}
    />
  );
}

export default MovieCreditPoster;
