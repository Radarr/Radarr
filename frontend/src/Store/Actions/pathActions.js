import { createAction } from 'redux-actions';
import { createThunk, handleThunks } from 'Store/thunks';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import { set } from './baseActions';
import createHandleActions from './Creators/createHandleActions';

//
// Variables

export const section = 'paths';

const LISTING_CACHE_TTL = 5000;
const LISTING_CACHE_MAX_SIZE = 256;
const listingCache = new Map();
const inFlightListings = new Map();
let latestFetchId = 0;

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
const createClearPathsAction = createAction(CLEAR_PATHS);

export function clearPaths() {
  latestFetchId++;

  return createClearPathsAction();
}

function getCachedListing(cacheKey) {
  const now = Date.now();

  listingCache.forEach(({ expiresAt }, key) => {
    if (expiresAt <= now) {
      listingCache.delete(key);
    }
  });

  const cached = listingCache.get(cacheKey);

  if (!cached) {
    return undefined;
  }

  // Refresh insertion order so the size bound evicts the least recently used
  // listing first.
  listingCache.delete(cacheKey);
  listingCache.set(cacheKey, cached);

  return cached.data;
}

function cacheListing(cacheKey, data) {
  listingCache.delete(cacheKey);
  listingCache.set(cacheKey, {
    data,
    expiresAt: Date.now() + LISTING_CACHE_TTL
  });

  while (listingCache.size > LISTING_CACHE_MAX_SIZE) {
    const oldestKey = listingCache.keys().next().value;

    listingCache.delete(oldestKey);
  }
}

//
// Action Handlers

export const actionHandlers = handleThunks({

  [FETCH_PATHS]: function(getState, payload, dispatch) {
    const fetchId = ++latestFetchId;

    dispatch(set({ section, isFetching: true }));

    const {
      path,
      allowFoldersWithoutTrailingSlashes = false,
      includeFiles = false
    } = payload;

    const fetchChildren = (queryPath) => {
      const cacheKey = `${includeFiles}:${allowFoldersWithoutTrailingSlashes}:${queryPath}`;
      const cached = getCachedListing(cacheKey);

      if (cached !== undefined) {
        return Promise.resolve(cached);
      }

      const inFlight = inFlightListings.get(cacheKey);

      if (inFlight) {
        return inFlight;
      }

      const listingPromise = Promise.resolve(
        createAjaxRequest({
          url: '/filesystem',
          data: {
            path: queryPath,
            allowFoldersWithoutTrailingSlashes,
            includeFiles
          }
        }).request
      )
        .then((data) => {
          cacheListing(cacheKey, data);

          return data;
        }, () => {
          return { parent: '', directories: [], files: [] };
        })
        .finally(() => {
          if (inFlightListings.get(cacheKey) === listingPromise) {
            inFlightListings.delete(cacheKey);
          }
        });

      inFlightListings.set(cacheKey, listingPromise);

      return listingPromise;
    };

    // Resolve one directory level at a time. Intermediate segments must be
    // exact, so fuzzy input cannot fan out into multiple child listings.
    let rootPath = '/';
    let pathWithoutRoot = path;

    if ((/^[a-zA-Z]:[\\/]/).test(path)) {
      rootPath = path.slice(0, 3);
      pathWithoutRoot = path.slice(3);
    } else if (path.startsWith('\\')) {
      rootPath = '\\';
      pathWithoutRoot = path.slice(1);
    }

    const segments = pathWithoutRoot
      .split(/[\\/]/)
      .filter((segment) => segment.length);
    const hasTrailingSeparator = (/[\\/]$/).test(path);

    const resolve = async() => {
      if (!segments.length) {
        return fetchChildren(rootPath);
      }

      let queryPath = rootPath;
      let parent = '';
      let results = [];

      for (let i = 0; i < segments.length; i++) {
        const segment = segments[i].toLowerCase();
        const isLast = i === segments.length - 1;

        const listing = await fetchChildren(queryPath);
        const { directories, files } = listing;
        parent = listing.parent;

        const matched = directories.filter(({ name }) =>
          name.toLowerCase().includes(segment)
        );

        if (isLast) {
          if (includeFiles) {
            matched.push(
              ...files.filter(({ name }) => name.toLowerCase().includes(segment))
            );
          }

          if (!hasTrailingSeparator) {
            results = matched;
            break;
          }

          // A trailing separator lists children only when the directory name
          // is unambiguous. Partial or ambiguous names remain suggestions.
          const exactDirs = matched.filter(
            ({ name, type }) => type === 'folder' && name.toLowerCase() === segment
          );

          if (exactDirs.length !== 1) {
            results = matched;
            break;
          }

          const subListing = await fetchChildren(exactDirs[0].path);
          parent = subListing.parent;
          results = includeFiles ?
            [...subListing.directories, ...subListing.files] :
            subListing.directories;
        } else {
          const matchedFolders = matched.filter(({ type }) => type === 'folder');
          const exactFolders = matchedFolders.filter(
            ({ name }) => name.toLowerCase() === segment
          );

          if (exactFolders.length !== 1) {
            results = matchedFolders;
            break;
          }

          queryPath = exactFolders[0].path;
        }
      }

      return {
        parent,
        directories: results.filter(({ type }) => type === 'folder'),
        files: results.filter(({ type }) => type === 'file')
      };
    };

    resolve().then((data) => {
      if (fetchId !== latestFetchId) {
        return;
      }

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

  [CLEAR_PATHS]: (state) => {
    const newState = Object.assign({}, state);

    newState.currentPath = '';
    newState.isFetching = false;
    newState.isPopulated = false;
    newState.error = null;
    newState.directories = [];
    newState.files = [];
    newState.parent = '';

    return newState;
  }

}, defaultState, section);
