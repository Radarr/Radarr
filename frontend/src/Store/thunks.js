const thunks = {};

export function createThunk(type) {
  return function(payload = {}) {
    return function(dispatch, getState) {
      const thunk = thunks[type];

      if (thunk) {
        return thunk(getState, payload, dispatch);
      }

      throw Error(`Thunk handler has not been registered for ${type}`);
    };
  };
}

export function handleThunks(handlers) {
  const types = Object.keys(handlers);

  types.forEach((type) => {
    thunks[type] = handlers[type];
  });
}

