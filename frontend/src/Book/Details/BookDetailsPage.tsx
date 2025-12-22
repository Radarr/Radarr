import React, { useEffect } from 'react';
import { useSelector } from 'react-redux';
import { useHistory, useParams } from 'react-router';
import NotFound from 'Components/NotFound';
import usePrevious from 'Helpers/Hooks/usePrevious';
import createAllBooksSelector from 'Store/Selectors/createAllBooksSelector';
import translate from 'Utilities/String/translate';
import BookDetails from './BookDetails';

function BookDetailsPage() {
  const allBooks = useSelector(createAllBooksSelector());
  const { id } = useParams<{ id: string }>();
  const history = useHistory();

  const bookId = parseInt(id);
  const bookIndex = allBooks.findIndex((book) => book.id === bookId);

  const previousIndex = usePrevious(bookIndex);

  useEffect(() => {
    if (
      bookIndex === -1 &&
      previousIndex !== -1 &&
      previousIndex !== undefined
    ) {
      history.push(`${window.Radarr.urlBase}/books`);
    }
  }, [bookIndex, previousIndex, history]);

  if (bookIndex === -1) {
    return <NotFound message={translate('BookCannotBeFound')} />;
  }

  return <BookDetails bookId={allBooks[bookIndex].id} />;
}

export default BookDetailsPage;
