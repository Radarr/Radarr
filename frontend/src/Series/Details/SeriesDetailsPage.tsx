import React, { useEffect } from 'react';
import { useSelector } from 'react-redux';
import { useHistory, useParams } from 'react-router';
import NotFound from 'Components/NotFound';
import usePrevious from 'Helpers/Hooks/usePrevious';
import createAllSeriesSelector from 'Store/Selectors/createAllSeriesSelector';
import translate from 'Utilities/String/translate';
import SeriesDetails from './SeriesDetails';

function SeriesDetailsPage() {
  const allSeries = useSelector(createAllSeriesSelector());
  const { id } = useParams<{ id: string }>();
  const history = useHistory();

  const seriesId = Number.parseInt(id);
  const seriesIndex = allSeries.findIndex((series) => series.id === seriesId);

  const previousIndex = usePrevious(seriesIndex);

  useEffect(() => {
    if (
      seriesIndex === -1 &&
      previousIndex !== -1 &&
      previousIndex !== undefined
    ) {
      history.push(`${window.Radarr.urlBase}/series`);
    }
  }, [seriesIndex, previousIndex, history]);

  if (seriesIndex === -1) {
    return <NotFound message={translate('SeriesCannotBeFound')} />;
  }

  return <SeriesDetails seriesId={allSeries[seriesIndex].id} />;
}

export default SeriesDetailsPage;
