import React, { useEffect } from 'react';
import { useSelector } from 'react-redux';
import { useHistory, useParams } from 'react-router';
import NotFound from 'Components/NotFound';
import usePrevious from 'Helpers/Hooks/usePrevious';
import createAllBookSeriesSelector from 'Store/Selectors/createAllBookSeriesSelector';
import translate from 'Utilities/String/translate';
import BookSeriesDetails from './BookSeriesDetails';

function BookSeriesDetailsPage() {
  const allBookSeries = useSelector(createAllBookSeriesSelector());
  const { id } = useParams<{ id: string }>();
  const history = useHistory();

  const bookSeriesId = Number.parseInt(id);
  const bookSeriesIndex = allBookSeries.findIndex(
    (bookSeries) => bookSeries.id === bookSeriesId
  );

  const previousIndex = usePrevious(bookSeriesIndex);

  useEffect(() => {
    if (
      bookSeriesIndex === -1 &&
      previousIndex !== -1 &&
      previousIndex !== undefined
    ) {
      history.push(`${window.Radarr.urlBase}/bookseries`);
    }
  }, [bookSeriesIndex, previousIndex, history]);

  if (bookSeriesIndex === -1) {
    return <NotFound message={translate('BookSeriesCannotBeFound')} />;
  }

  return <BookSeriesDetails bookSeriesId={allBookSeries[bookSeriesIndex].id} />;
}

export default BookSeriesDetailsPage;
