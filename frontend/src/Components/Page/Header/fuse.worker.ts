import Fuse from 'fuse.js';
import { SuggestedMovie } from './MovieSearchInput';

const fuseOptions = {
  shouldSort: true,
  includeMatches: true,
  ignoreLocation: true,
  threshold: 0.3,
  minMatchCharLength: 1,
  keys: [
    'title',
    'originalTitle',
    'year',
    'alternateTitles.title',
    'tmdbId',
    'imdbId',
    'tags.label',
  ],
};

let movies: SuggestedMovie[] = [];
let fuse: Fuse<SuggestedMovie> | null = null;

function getSuggestions(value: string) {
  const limit = 10;
  let suggestions = [];

  if (value.length === 1) {
    for (let i = 0; i < movies.length; i++) {
      const m = movies[i];
      if (m.firstCharacter === value.toLowerCase()) {
        suggestions.push({
          item: movies[i],
          indices: [[0, 0]],
          matches: [
            {
              value: m.title,
              key: 'title',
            },
          ],
          refIndex: 0,
        });
        if (suggestions.length > limit) {
          break;
        }
      }
    }
  } else {
    fuse ??= new Fuse(movies, fuseOptions);

    suggestions = fuse.search(value, { limit });
  }

  return suggestions;
}

onmessage = function (e) {
  if (!e) {
    return;
  }

  if (e.data.movies) {
    movies = e.data.movies;
    fuse = null;
    return;
  }

  const { value } = e.data;

  const suggestions = getSuggestions(value);

  const results = {
    value,
    suggestions,
  };

  self.postMessage(results);
};
