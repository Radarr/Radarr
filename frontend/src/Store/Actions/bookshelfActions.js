import { createAction } from 'redux-actions';
import { filterBuilderTypes, filterBuilderValueTypes, sortDirections } from 'Helpers/Props';
import { createThunk, handleThunks } from 'Store/thunks';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import { filterPredicates, filters } from './authorActions';
import { set } from './baseActions';
import { fetchBooks } from './bookActions';
import createHandleActions from './Creators/createHandleActions';
import createSetClientSideCollectionFilterReducer from './Creators/Reducers/createSetClientSideCollectionFilterReducer';
import createSetClientSideCollectionSortReducer from './Creators/Reducers/createSetClientSideCollectionSortReducer';

//
// Variables

export const section = 'bookshelf';

//
// State

export const defaultState = {
  isSaving: false,
  saveError: null,
  sortKey: 'sortName',
  sortDirection: sortDirections.ASCENDING,
  secondarySortKey: 'sortName',
  secondarySortDirection: sortDirections.ASCENDING,
  selectedFilterKey: 'all',
  filters,
  filterPredicates,

  filterBuilderProps: [
    {
      name: 'monitored',
      label: 'Monitored',
      type: filterBuilderTypes.EXACT,
      valueType: filterBuilderValueTypes.BOOL
    },
    {
      name: 'status',
      label: 'Status',
      type: filterBuilderTypes.EXACT,
      valueType: filterBuilderValueTypes.AUTHOR_STATUS
    },
    {
      name: 'authorType',
      label: 'Author Type',
      type: filterBuilderTypes.EXACT
    },
    {
      name: 'qualityProfileId',
      label: 'Quality Profile',
      type: filterBuilderTypes.EXACT,
      valueType: filterBuilderValueTypes.QUALITY_PROFILE
    },
    {
      name: 'metadataProfileId',
      label: 'Metadata Profile',
      type: filterBuilderTypes.EXACT,
      valueType: filterBuilderValueTypes.METADATA_PROFILE
    },
    {
      name: 'rootFolderPath',
      label: 'Root Folder Path',
      type: filterBuilderTypes.EXACT
    },
    {
      name: 'tags',
      label: 'Tags',
      type: filterBuilderTypes.ARRAY,
      valueType: filterBuilderValueTypes.TAG
    }
  ]
};

export const persistState = [
  'bookshelf.sortKey',
  'bookshelf.sortDirection',
  'bookshelf.selectedFilterKey',
  'bookshelf.customFilters'
];

//
// Actions Types

export const SET_BOOKSHELF_SORT = 'bookshelf/setBookshelfSort';
export const SET_BOOKSHELF_FILTER = 'bookshelf/setBookshelfFilter';
export const SAVE_BOOKSHELF = 'bookshelf/saveBookshelf';

//
// Action Creators

export const setBookshelfSort = createAction(SET_BOOKSHELF_SORT);
export const setBookshelfFilter = createAction(SET_BOOKSHELF_FILTER);
export const saveBookshelf = createThunk(SAVE_BOOKSHELF);

//
// Action Handlers

export const actionHandlers = handleThunks({

  [SAVE_BOOKSHELF]: function(getState, payload, dispatch) {
    const {
      authorIds,
      monitored,
      monitor
    } = payload;

    const authors = [];

    authorIds.forEach((id) => {
      const authorToUpdate = { id };

      if (payload.hasOwnProperty('monitored')) {
        authorToUpdate.monitored = monitored;
      }

      authors.push(authorToUpdate);
    });

    dispatch(set({
      section,
      isSaving: true
    }));

    const promise = createAjaxRequest({
      url: '/bookshelf',
      method: 'POST',
      data: JSON.stringify({
        authors,
        monitoringOptions: { monitor }
      }),
      dataType: 'json'
    }).request;

    promise.done((data) => {
      dispatch(fetchBooks());

      dispatch(set({
        section,
        isSaving: false,
        saveError: null
      }));
    });

    promise.fail((xhr) => {
      dispatch(set({
        section,
        isSaving: false,
        saveError: xhr
      }));
    });
  }
});

//
// Reducers

export const reducers = createHandleActions({

  [SET_BOOKSHELF_SORT]: createSetClientSideCollectionSortReducer(section),
  [SET_BOOKSHELF_FILTER]: createSetClientSideCollectionFilterReducer(section)

}, defaultState, section);

