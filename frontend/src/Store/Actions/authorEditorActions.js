import { createAction } from 'redux-actions';
import { batchActions } from 'redux-batched-actions';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import { filterBuilderTypes, filterBuilderValueTypes, sortDirections } from 'Helpers/Props';
import { createThunk, handleThunks } from 'Store/thunks';
import createSetClientSideCollectionSortReducer from './Creators/Reducers/createSetClientSideCollectionSortReducer';
import createSetClientSideCollectionFilterReducer from './Creators/Reducers/createSetClientSideCollectionFilterReducer';
import createHandleActions from './Creators/createHandleActions';
import { set, updateItem } from './baseActions';
import { filters, filterPredicates, sortPredicates } from './authorActions';

//
// Variables

export const section = 'authorEditor';

//
// State

export const defaultState = {
  isSaving: false,
  saveError: null,
  isDeleting: false,
  deleteError: null,
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
      name: 'path',
      label: 'Path',
      type: filterBuilderTypes.STRING
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
  ],

  sortPredicates
};

export const persistState = [
  'authorEditor.sortKey',
  'authorEditor.sortDirection',
  'authorEditor.selectedFilterKey',
  'authorEditor.customFilters'
];

//
// Actions Types

export const SET_AUTHOR_EDITOR_SORT = 'authorEditor/setAuthorEditorSort';
export const SET_AUTHOR_EDITOR_FILTER = 'authorEditor/setAuthorEditorFilter';
export const SAVE_AUTHOR_EDITOR = 'authorEditor/saveAuthorEditor';
export const BULK_DELETE_AUTHOR = 'authorEditor/bulkDeleteAuthor';

//
// Action Creators

export const setAuthorEditorSort = createAction(SET_AUTHOR_EDITOR_SORT);
export const setAuthorEditorFilter = createAction(SET_AUTHOR_EDITOR_FILTER);
export const saveAuthorEditor = createThunk(SAVE_AUTHOR_EDITOR);
export const bulkDeleteAuthor = createThunk(BULK_DELETE_AUTHOR);

//
// Action Handlers

export const actionHandlers = handleThunks({
  [SAVE_AUTHOR_EDITOR]: function(getState, payload, dispatch) {
    dispatch(set({
      section,
      isSaving: true
    }));

    const promise = createAjaxRequest({
      url: '/author/editor',
      method: 'PUT',
      data: JSON.stringify(payload),
      dataType: 'json'
    }).request;

    promise.done((data) => {
      dispatch(batchActions([
        ...data.map((author) => {
          return updateItem({
            id: author.id,
            section: 'authors',
            ...author
          });
        }),

        set({
          section,
          isSaving: false,
          saveError: null
        })
      ]));
    });

    promise.fail((xhr) => {
      dispatch(set({
        section,
        isSaving: false,
        saveError: xhr
      }));
    });
  },

  [BULK_DELETE_AUTHOR]: function(getState, payload, dispatch) {
    dispatch(set({
      section,
      isDeleting: true
    }));

    const promise = createAjaxRequest({
      url: '/author/editor',
      method: 'DELETE',
      data: JSON.stringify(payload),
      dataType: 'json'
    }).request;

    promise.done(() => {
      // SignalR will take care of removing the author from the collection

      dispatch(set({
        section,
        isDeleting: false,
        deleteError: null
      }));
    });

    promise.fail((xhr) => {
      dispatch(set({
        section,
        isDeleting: false,
        deleteError: xhr
      }));
    });
  }
});

//
// Reducers

export const reducers = createHandleActions({

  [SET_AUTHOR_EDITOR_SORT]: createSetClientSideCollectionSortReducer(section),
  [SET_AUTHOR_EDITOR_FILTER]: createSetClientSideCollectionFilterReducer(section)

}, defaultState, section);
