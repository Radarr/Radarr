import _ from 'lodash';
import { createAction } from 'redux-actions';
import { batchActions } from 'redux-batched-actions';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import { sortDirections } from 'Helpers/Props';
import { createThunk, handleThunks } from 'Store/thunks';
import createSetTableOptionReducer from './Creators/Reducers/createSetTableOptionReducer';
import createSetClientSideCollectionSortReducer from './Creators/Reducers/createSetClientSideCollectionSortReducer';
import createClearReducer from './Creators/Reducers/createClearReducer';
import bookEntities from 'Book/bookEntities';
import createFetchHandler from './Creators/createFetchHandler';
import createHandleActions from './Creators/createHandleActions';
import createRemoveItemHandler from './Creators/createRemoveItemHandler';
import { set, removeItem, updateItem } from './baseActions';

//
// Variables

export const section = 'bookFiles';

//
// State

export const defaultState = {
  isFetching: false,
  isPopulated: false,
  sortKey: 'path',
  sortDirection: sortDirections.ASCENDING,

  error: null,
  isDeleting: false,
  deleteError: null,
  isSaving: false,
  saveError: null,
  items: [],

  sortPredicates: {
    quality: function(item, direction) {
      return item.quality ? item.qualityWeight : 0;
    }
  },

  columns: [
    {
      name: 'path',
      label: 'Path',
      isSortable: true,
      isVisible: true,
      isModifiable: false
    },
    {
      name: 'size',
      label: 'Size',
      isSortable: true,
      isVisible: true
    },
    {
      name: 'dateAdded',
      label: 'Date Added',
      isSortable: true,
      isVisible: true
    },
    {
      name: 'quality',
      label: 'Quality',
      isSortable: true,
      isVisible: true
    },
    {
      name: 'actions',
      columnLabel: 'Actions',
      isVisible: true,
      isModifiable: false
    }
  ]
};

export const persistState = [
  'bookFiles.sortKey',
  'bookFiles.sortDirection'
];

//
// Actions Types

export const FETCH_BOOK_FILES = 'bookFiles/fetchBookFiles';
export const DELETE_BOOK_FILE = 'bookFiles/deleteBookFile';
export const DELETE_BOOK_FILES = 'bookFiles/deleteBookFiles';
export const UPDATE_BOOK_FILES = 'bookFiles/updateBookFiles';
export const SET_BOOK_FILES_SORT = 'bookFiles/setBookFilesSort';
export const SET_BOOK_FILES_TABLE_OPTION = 'bookFiles/setBookFilesTableOption';
export const CLEAR_BOOK_FILES = 'bookFiles/clearBookFiles';

//
// Action Creators

export const fetchBookFiles = createThunk(FETCH_BOOK_FILES);
export const deleteBookFile = createThunk(DELETE_BOOK_FILE);
export const deleteBookFiles = createThunk(DELETE_BOOK_FILES);
export const updateBookFiles = createThunk(UPDATE_BOOK_FILES);
export const setBookFilesSort = createAction(SET_BOOK_FILES_SORT);
export const setBookFilesTableOption = createAction(SET_BOOK_FILES_TABLE_OPTION);
export const clearBookFiles = createAction(CLEAR_BOOK_FILES);

//
// Helpers

const deleteBookFileHelper = createRemoveItemHandler(section, '/bookFile');

//
// Action Handlers

export const actionHandlers = handleThunks({
  [FETCH_BOOK_FILES]: createFetchHandler(section, '/bookFile'),

  [DELETE_BOOK_FILE]: function(getState, payload, dispatch) {
    const {
      id: bookFileId,
      bookEntity: bookEntity = bookEntities.BOOKS
    } = payload;

    const bookSection = _.last(bookEntity.split('.'));
    const deletePromise = deleteBookFileHelper(getState, payload, dispatch);

    deletePromise.done(() => {
      const books = getState().books.items;
      const booksWithRemovedFiles = _.filter(books, { bookFileId });

      dispatch(batchActions([
        ...booksWithRemovedFiles.map((book) => {
          return updateItem({
            section: bookSection,
            ...book,
            bookFileId: 0,
            hasFile: false
          });
        })
      ]));
    });
  },

  [DELETE_BOOK_FILES]: function(getState, payload, dispatch) {
    const {
      bookFileIds: bookFileIds
    } = payload;

    dispatch(set({ section, isDeleting: true }));

    const promise = createAjaxRequest({
      url: '/bookFile/bulk',
      method: 'DELETE',
      dataType: 'json',
      data: JSON.stringify({ bookFileIds })
    }).request;

    promise.done(() => {
      const books = getState().books.items;
      const booksWithRemovedFiles = bookFileIds.reduce((acc, bookFileId) => {
        acc.push(..._.filter(books, { bookFileId }));

        return acc;
      }, []);

      dispatch(batchActions([
        ...bookFileIds.map((id) => {
          return removeItem({ section, id });
        }),

        ...booksWithRemovedFiles.map((book) => {
          return updateItem({
            section: 'books',
            ...book,
            bookFileId: 0,
            hasFile: false
          });
        }),

        set({
          section,
          isDeleting: false,
          deleteError: null
        })
      ]));
    });

    promise.fail((xhr) => {
      dispatch(set({
        section,
        isDeleting: false,
        deleteError: xhr
      }));
    });
  },

  [UPDATE_BOOK_FILES]: function(getState, payload, dispatch) {
    const {
      bookFileIds,
      quality
    } = payload;

    dispatch(set({ section, isSaving: true }));

    const data = {
      bookFileIds
    };

    if (quality) {
      data.quality = quality;
    }

    const promise = createAjaxRequest({
      url: '/bookFile/editor',
      method: 'PUT',
      dataType: 'json',
      data: JSON.stringify(data)
    }).request;

    promise.done(() => {
      dispatch(batchActions([
        ...bookFileIds.map((id) => {
          const props = {};

          if (quality) {
            props.quality = quality;
          }

          return updateItem({ section, id, ...props });
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
  }
});

//
// Reducers

export const reducers = createHandleActions({
  [SET_BOOK_FILES_SORT]: createSetClientSideCollectionSortReducer(section),
  [SET_BOOK_FILES_TABLE_OPTION]: createSetTableOptionReducer(section),

  [CLEAR_BOOK_FILES]: createClearReducer(section, {
    isFetching: false,
    isPopulated: false,
    error: null,
    items: []
  })

}, defaultState, section);
