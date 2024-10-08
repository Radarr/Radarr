import Fuse from 'fuse.js';

const fuseOptions = {
  shouldSort: true,
  includeMatches: true,
  ignoreLocation: true,
  threshold: 0.3,
  minMatchCharLength: 1,
  keys: [
    'title',
    'alternateTitles.title',
    'tmdbId',
    'imdbId',
    'tags.label'
  ]
};

function getSuggestions(movies, value) {
  const limit = 10;
  let suggestions = [];

  if (value.length === 1) {
    for (let i = 0; i < movies.length; i++) {
      const s = movies[i];
      if (s.firstCharacter === value.toLowerCase()) {
        suggestions.push({
          item: movies[i],
          indices: [
            [0, 0]
          ],
          matches: [
            {
              value: s.title,
              key: 'title'
            }
          ],
          refIndex: 0
        });
        if (suggestions.length > limit) {
          break;
        }
      }
    }
  } else {
    const fuse = new Fuse(movies, fuseOptions);
    suggestions = fuse.search(value, { limit });
  }

  return suggestions;
}

onmessage = function(e) {
  if (!e) {
    return;
  }

  const {
    movies,
    value
  } = e.data;

  const suggestions = getSuggestions(movies, value);

  const results = {
    value,
    suggestions
  };

  self.postMessage(results);
};
