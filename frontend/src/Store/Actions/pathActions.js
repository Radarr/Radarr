import { createAction } from 'redux-actions';
import { createThunk, handleThunks } from 'Store/thunks';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import { set } from './baseActions';
import createHandleActions from './Creators/createHandleActions';

//
// Variables

export const section = 'paths';

//
// State

export const defaultState = {
  currentPath: '',
  isPopulated: false,
  isFetching: false,
  error: null,
  directories: [],
  files: [],
  parent: null
};

//
// Actions Types

export const FETCH_PATHS = 'paths/fetchPaths';
export const UPDATE_PATHS = 'paths/updatePaths';
export const CLEAR_PATHS = 'paths/clearPaths';

//
// Action Creators

export const fetchPaths = createThunk(FETCH_PATHS);
export const updatePaths = createAction(UPDATE_PATHS);
export const clearPaths = createAction(CLEAR_PATHS);

// Short-lived listing cache: segment resolution re-fetches the same
// parent directories on nearly every keystroke.
const LISTING_CACHE_TTL = 5000;
const listingCache = new Map();

//
// Action Handlers

export const actionHandlers = handleThunks({

  [FETCH_PATHS]: function(getState, payload, dispatch) {
    dispatch(set({ section, isFetching: true }));

    const {
      path,
      allowFoldersWithoutTrailingSlashes = false,
      includeFiles = false
    } = payload;

    const fetchChildren = (queryPath) => {
      const cacheKey = `${includeFiles}:${allowFoldersWithoutTrailingSlashes}:${queryPath}`;
      const cached = listingCache.get(cacheKey);

      if (cached && Date.now() - cached.timestamp < LISTING_CACHE_TTL) {
        return Promise.resolve(cached.data);
      }

      return createAjaxRequest({
        url: '/filesystem',
        data: {
          path: queryPath,
          allowFoldersWithoutTrailingSlashes,
          includeFiles
        }
      }).request.then(
        (data) => {
          listingCache.set(cacheKey, { data, timestamp: Date.now() });

          return data;
        },
        () => ({ parent: '', directories: [], files: [] })
      );
    };

    // Resolve the query segment by segment: each segment is a
    // case-insensitive containment filter for its directory level, so
    // `/down/dbd` descends into `/downloads/` and matches `[DBD-Raws]...`.
    let rootPath = '/';

    if ((/^[a-zA-Z]:[\\/]/).test(path)) {
      rootPath = path.slice(0, 3);
    } else if (path.startsWith('\\')) {
      rootPath = '\\';
    }

    const segments = path.split(/[\\/]/).filter((segment) => segment.length);
    const hasTrailingSeparator = (/[\\/]$/).test(path);

    const resolve = async() => {
      if (!segments.length) {
        return fetchChildren(rootPath);
      }

      let parents = [rootPath];
      let parent = '';
      let results = [];

      for (let i = 0; i < segments.length; i++) {
        const segment = segments[i].toLowerCase();
        const isLast = i === segments.length - 1;

        const listings = await Promise.all(parents.map(fetchChildren));
        const directories = listings.flatMap((data) => data.directories);

        if (listings.length) {
          parent = listings[listings.length - 1].parent;
        }

        const matched = directories.filter(({ name }) =>
          name.toLowerCase().includes(segment)
        );

        if (isLast) {
          if (includeFiles) {
            const files = listings.flatMap((data) => data.files);
            matched.push(
              ...files.filter(({ name }) => name.toLowerCase().includes(segment))
            );
          }

          if (!hasTrailingSeparator) {
            results = matched;
            break;
          }

          // Trailing separator after an exact directory name lists its
          // children (`/config/`); after a partial name the match itself
          // stays the suggestion (`/downloads/Kung/`).
          const exactDirs = matched.filter(
            ({ name, type }) => type === 'folder' && name.toLowerCase() === segment
          );
          results = matched.filter((entry) => !exactDirs.includes(entry));

          if (exactDirs.length) {
            const subListings = await Promise.all(
              exactDirs.map(({ path: dirPath }) => fetchChildren(dirPath))
            );
            parent = subListings[subListings.length - 1].parent;
            results.push(
              ...subListings.flatMap((data) =>
                (includeFiles ? [...data.directories, ...data.files] : data.directories)
              )
            );
          }
        } else {
          parents = matched
            .filter(({ type }) => type === 'folder')
            .map(({ path: matchedPath }) => matchedPath);
        }
      }

      return {
        parent,
        directories: results.filter(({ type }) => type === 'folder'),
        files: results.filter(({ type }) => type === 'file')
      };
    };

    resolve().then((data) => {
      // `currentPath` stays empty so the prefix-based selectors pass the
      // resolved entries through; matching happens against the typed value.
      dispatch(updatePaths({ path: '', ...data }));

      dispatch(set({
        section,
        isFetching: false,
        isPopulated: true,
        error: null
      }));
    });
  }

});

//
// Reducers

export const reducers = createHandleActions({

  [UPDATE_PATHS]: (state, { payload }) => {
    const newState = Object.assign({}, state);

    newState.currentPath = payload.path;
    newState.directories = payload.directories;
    newState.files = payload.files;
    newState.parent = payload.parent;

    return newState;
  },

  [CLEAR_PATHS]: (state, { payload }) => {
    const newState = Object.assign({}, state);

    newState.path = '';
    newState.directories = [];
    newState.files = [];
    newState.parent = '';

    return newState;
  }

}, defaultState, section);
