import React, { useEffect } from 'react';
import { useSelector } from 'react-redux';
import { useHistory, useParams } from 'react-router';
import NotFound from 'Components/NotFound';
import usePrevious from 'Helpers/Hooks/usePrevious';
import createAllAudiobooksSelector from 'Store/Selectors/createAllAudiobooksSelector';
import translate from 'Utilities/String/translate';
import AudiobookDetails from './AudiobookDetails';

function AudiobookDetailsPage() {
  const allAudiobooks = useSelector(createAllAudiobooksSelector());
  const { id } = useParams<{ id: string }>();
  const history = useHistory();

  const audiobookId = Number.parseInt(id);
  const audiobookIndex = allAudiobooks.findIndex(
    (audiobook) => audiobook.id === audiobookId
  );

  const previousIndex = usePrevious(audiobookIndex);

  useEffect(() => {
    if (
      audiobookIndex === -1 &&
      previousIndex !== -1 &&
      previousIndex !== undefined
    ) {
      history.push(`${window.Radarr.urlBase}/audiobooks`);
    }
  }, [audiobookIndex, previousIndex, history]);

  if (audiobookIndex === -1) {
    return <NotFound message={translate('AudiobookCannotBeFound')} />;
  }

  return <AudiobookDetails audiobookId={allAudiobooks[audiobookIndex].id} />;
}

export default AudiobookDetailsPage;
