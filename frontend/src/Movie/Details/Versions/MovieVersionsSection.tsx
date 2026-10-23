import React, { useCallback } from 'react';
import FieldSet from 'Components/FieldSet';
import useApiQuery from 'Helpers/Hooks/useApiQuery';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import translate from 'Utilities/String/translate';
import AddMovieVersionModal from './AddMovieVersionModal';
import MovieVersion from './MovieVersion';
import MovieVersionsTable from './MovieVersionsTable';

interface MovieVersionsSectionProps {
  movieId: number;
  isAddVersionModalOpen: boolean;
  onAddVersionModalClose(): void;
}

function MovieVersionsSection({
  movieId,
  isAddVersionModalOpen,
  onAddVersionModalClose,
}: MovieVersionsSectionProps) {
  const { data, refetch } = useApiQuery<MovieVersion[]>({
    path: `/movieversion?movieId=${movieId}`,
  });

  const versions = data ?? [];

  const handleVersionAdded = useCallback(() => {
    onAddVersionModalClose();
    refetch();
  }, [onAddVersionModalClose, refetch]);

  const handleRemovePress = useCallback(
    (id: number) => {
      const { request } = createAjaxRequest({
        url: `/movieversion/${id}`,
        method: 'DELETE',
        dataType: 'json',
      });

      request.always(() => {
        refetch();
      });
    },
    [refetch]
  );

  return (
    <>
      {/* A movie with a single version looks exactly as before */}
      {versions.length > 1 ? (
        <FieldSet legend={translate('MovieVersions')}>
          <MovieVersionsTable
            versions={versions}
            onRemovePress={handleRemovePress}
          />
        </FieldSet>
      ) : null}

      <AddMovieVersionModal
        isOpen={isAddVersionModalOpen}
        movieId={movieId}
        onVersionAdded={handleVersionAdded}
        onModalClose={onAddVersionModalClose}
      />
    </>
  );
}

export default MovieVersionsSection;
