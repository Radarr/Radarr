import React, { useEffect } from 'react';
import { useSelector } from 'react-redux';
import { useHistory, useParams } from 'react-router';
import NotFound from 'Components/NotFound';
import usePrevious from 'Helpers/Hooks/usePrevious';
import createAllAuthorsSelector from 'Store/Selectors/createAllAuthorsSelector';
import translate from 'Utilities/String/translate';
import AuthorDetails from './AuthorDetails';

function AuthorDetailsPage() {
  const allAuthors = useSelector(createAllAuthorsSelector());
  const { id } = useParams<{ id: string }>();
  const history = useHistory();

  const authorId = Number.parseInt(id);
  const authorIndex = allAuthors.findIndex((author) => author.id === authorId);

  const previousIndex = usePrevious(authorIndex);

  useEffect(() => {
    if (
      authorIndex === -1 &&
      previousIndex !== -1 &&
      previousIndex !== undefined
    ) {
      history.push(`${window.Radarr.urlBase}/authors`);
    }
  }, [authorIndex, previousIndex, history]);

  if (authorIndex === -1) {
    return <NotFound message={translate('AuthorCannotBeFound')} />;
  }

  return <AuthorDetails authorId={allAuthors[authorIndex].id} />;
}

export default AuthorDetailsPage;
