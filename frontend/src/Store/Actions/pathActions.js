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
const MAX_CONCURRENT_LISTING_REQUESTS = 8;
const MIN_FUZZY_INTERMEDIATE_SEGMENT_LENGTH = 2;
const MAX_FUZZY_INTERMEDIATE_MATCHES = 20;
const listingCache = new Map();
const inFlightListings = new Map();
const listingRequestQueue = [];
let activeListingRequests = 0;
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

function startListingRequest({ requestFactory, resolve, reject }) {
  activeListingRequests++;

  Promise.resolve()
    .then(requestFactory)
    .then(resolve, reject)
    .finally(() => {
      activeListingRequests--;
      runListingRequestQueue();
    });
}

function runListingRequestQueue() {
  while (
    activeListingRequests < MAX_CONCURRENT_LISTING_REQUESTS &&
    listingRequestQueue.length
  ) {
    startListingRequest(listingRequestQueue.shift());
  }
}

function scheduleListingRequest(requestFactory) {
  return new Promise((resolve, reject) => {
    listingRequestQueue.push({ requestFactory, resolve, reject });
    runListingRequestQueue();
  });
}

async function mapWithConcurrency(items, mapper) {
  const results = new Array(items.length);
  let nextIndex = 0;

  async function worker() {
    while (nextIndex < items.length) {
      const index = nextIndex++;

      results[index] = await mapper(items[index]);
    }
  }

  const workers = Array.from(
    { length: Math.min(MAX_CONCURRENT_LISTING_REQUESTS, items.length) },
    worker
  );

  await Promise.all(workers);

  return results;
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

      const listingPromise = scheduleListingRequest(() => {
        return createAjaxRequest({
          url: '/filesystem',
          data: {
            path: queryPath,
            allowFoldersWithoutTrailingSlashes,
            includeFiles
          }
        }).request;
      })
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
      const limitedResults = [];

      for (let i = 0; i < segments.length; i++) {
        const segment = segments[i].toLowerCase();
        const isLast = i === segments.length - 1;

        const listings = await mapWithConcurrency(parents, fetchChildren);
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
            const subListings = await mapWithConcurrency(
              exactDirs,
              ({ path: dirPath }) => fetchChildren(dirPath)
            );
            parent = subListings[subListings.length - 1].parent;
            results.push(
              ...subListings.flatMap((data) =>
                (includeFiles ? [...data.directories, ...data.files] : data.directories)
              )
            );
          }
        } else {
          const matchedFolders = matched.filter(({ type }) => type === 'folder');
          const exactFolders = matchedFolders.filter(
            ({ name }) => name.toLowerCase() === segment
          );
          const canFollowFuzzyMatches =
            segment.length >= MIN_FUZZY_INTERMEDIATE_SEGMENT_LENGTH &&
            matchedFolders.length <= MAX_FUZZY_INTERMEDIATE_MATCHES;

          // Exact path segments are safe to follow. Fuzzy intermediate
          // segments must be specific and bounded so a broad match cannot
          // turn the next segment into hundreds of filesystem requests.
          if (canFollowFuzzyMatches) {
            parents = matchedFolders.map(({ path: matchedPath }) => matchedPath);
          } else {
            limitedResults.push(
              ...matchedFolders.filter(
                ({ name }) => name.toLowerCase() !== segment
              )
            );

            if (!exactFolders.length) {
              break;
            }

            parents = exactFolders.map(({ path: matchedPath }) => matchedPath);
          }
        }
      }

      return {
        parent,
        directories: [...limitedResults, ...results].filter(
          ({ type }) => type === 'folder'
        ),
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
