import { createAction } from 'redux-actions';
import createHandleActions from './Creators/createHandleActions';

export const section = 'personalizedUi';

export const defaultState = {
  theme: 'system',
  accentColor: '#ffc230',
  density: 'comfortable',
  posterSize: 'medium',
  cardStyle: 'rounded',
  enableAnimations: true,
  enableBackdrops: false,
  isSidebarCollapsed: false,
  dashboard: {
    isEditing: false,
    widgets: [
      { id: 'recentlyAdded', isVisible: true },
      { id: 'upcomingReleases', isVisible: true },
      { id: 'missingMonitored', isVisible: true },
      { id: 'activeDownloads', isVisible: true },
      { id: 'attention', isVisible: true },
      { id: 'libraryStatistics', isVisible: true },
      { id: 'diskSpace', isVisible: true },
      { id: 'calendarPreview', isVisible: true }
    ]
  }
};

export const SET_PERSONALIZED_UI_VALUE = 'personalizedUi/setValue';
export const SET_DASHBOARD_WIDGETS = 'personalizedUi/setDashboardWidgets';
export const SET_DASHBOARD_EDITING = 'personalizedUi/setDashboardEditing';

export const setPersonalizedUiValue = createAction(SET_PERSONALIZED_UI_VALUE);
export const setDashboardWidgets = createAction(SET_DASHBOARD_WIDGETS);
export const setDashboardEditing = createAction(SET_DASHBOARD_EDITING);

export const reducers = createHandleActions({
  [SET_PERSONALIZED_UI_VALUE]: (state, { payload }) => ({ ...state, ...payload }),
  [SET_DASHBOARD_WIDGETS]: (state, { payload }) => ({
    ...state,
    dashboard: { ...state.dashboard, widgets: payload.widgets }
  }),
  [SET_DASHBOARD_EDITING]: (state, { payload }) => ({
    ...state,
    dashboard: { ...state.dashboard, isEditing: payload.isEditing }
  })
}, defaultState, section);

export const persistState = [
  'personalizedUi.theme',
  'personalizedUi.accentColor',
  'personalizedUi.density',
  'personalizedUi.posterSize',
  'personalizedUi.cardStyle',
  'personalizedUi.enableAnimations',
  'personalizedUi.enableBackdrops',
  'personalizedUi.isSidebarCollapsed',
  'personalizedUi.dashboard.widgets'
];
