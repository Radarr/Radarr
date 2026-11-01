import { useMemo } from 'react';
import { useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import UiSettings from 'typings/Settings/UiSettings';

export interface PosterReplacementColors {
  backgroundColor: string;
  textColor: string;
}

// Mirrors PosterReplacementService on the server, which replaces the posters of movies in the library.
// This is used for movies that aren't in the library, their posters come straight from TMDb.
export function getPosterReplacement(
  settings: Partial<UiSettings>,
  genres: string[] = [],
  tags: number[] = []
): PosterReplacementColors | null {
  if (!settings.posterReplacementEnabled) {
    return null;
  }

  const replacedGenres = (settings.posterReplacementGenres ?? []).map((g) =>
    g.toLowerCase()
  );
  const replacedTags = settings.posterReplacementTags ?? [];

  const isReplaced =
    genres.some((g) => replacedGenres.includes(g.toLowerCase())) ||
    tags.some((t) => replacedTags.includes(t));

  if (!isReplaced) {
    return null;
  }

  return {
    backgroundColor: settings.posterReplacementBackgroundColor ?? '#1c1c1c',
    textColor: settings.posterReplacementTextColor ?? '#ffffff',
  };
}

function usePosterReplacement(genres?: string[], tags?: number[]) {
  const settings = useSelector((state: AppState) => state.settings.ui.item);

  return useMemo(
    () => getPosterReplacement(settings, genres, tags),
    [settings, genres, tags]
  );
}

export default usePosterReplacement;
