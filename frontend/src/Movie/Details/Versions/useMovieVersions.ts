import { useCallback } from 'react';
import useApiQuery from 'Helpers/Hooks/useApiQuery';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import MovieVersion from './MovieVersion';

function useMovieVersions(movieId: number) {
  const { data, refetch } = useApiQuery<MovieVersion[]>({
    path: `/movieversion?movieId=${movieId}`,
  });

  const removeVersion = useCallback(
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

  return {
    versions: data ?? [],
    refetchVersions: refetch,
    removeVersion,
  };
}

export default useMovieVersions;
