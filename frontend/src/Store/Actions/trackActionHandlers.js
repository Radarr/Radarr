import createFetchHandler from './Creators/createFetchHandler';
import * as types from './actionTypes';

const section = 'tracks';

const trackActionHandlers = {
  [types.FETCH_TRACKS]: createFetchHandler(section, '/track')

};

export default trackActionHandlers;
