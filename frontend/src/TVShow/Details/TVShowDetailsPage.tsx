import React, { useEffect } from 'react';
import { useSelector } from 'react-redux';
import { useHistory, useParams } from 'react-router';
import NotFound from 'Components/NotFound';
import usePrevious from 'Helpers/Hooks/usePrevious';
import createAllTVShowsSelector from 'Store/Selectors/createAllTVShowsSelector';
import translate from 'Utilities/String/translate';
import TVShowDetails from './TVShowDetails';

function TVShowDetailsPage() {
  const allTVShows = useSelector(createAllTVShowsSelector());
  const { id } = useParams<{ id: string }>();
  const history = useHistory();

  const tvShowId = Number.parseInt(id);
  const tvShowIndex = allTVShows.findIndex((tvShow) => tvShow.id === tvShowId);

  const previousIndex = usePrevious(tvShowIndex);

  useEffect(() => {
    if (
      tvShowIndex === -1 &&
      previousIndex !== -1 &&
      previousIndex !== undefined
    ) {
      history.push(`${window.Radarr.urlBase}/tvshows`);
    }
  }, [tvShowIndex, previousIndex, history]);

  if (tvShowIndex === -1) {
    return <NotFound message={translate('TVShowCannotBeFound')} />;
  }

  return <TVShowDetails tvShowId={allTVShows[tvShowIndex].id} />;
}

export default TVShowDetailsPage;
