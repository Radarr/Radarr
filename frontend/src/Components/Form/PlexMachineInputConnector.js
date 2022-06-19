import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchPlexResources } from 'Store/Actions/settingsActions';
import PlexMachineInput from './PlexMachineInput';

function createMapStateToProps() {
  return createSelector(
    (state, { value }) => value,
    (state) => state.oAuth,
    (state) => state.settings.plex,
    (value, oAuth, plex) => {

      let values = [{ key: value, value }];
      let isDisabled = true;

      if (plex.isPopulated) {
        const serverValues = plex.items.filter((item) => item.provides.includes('server')).map((item) => {
          return ({
            key: item.clientIdentifier,
            value: `${item.name} / ${item.owned ? 'Owner' : 'User'} / ${item.clientIdentifier}`
          });
        });

        if (serverValues.find((item) => item.key === value)) {
          values = serverValues;
        } else {
          values = values.concat(serverValues);
        }

        isDisabled = false;
      }

      return ({
        accessToken: oAuth.result?.accessToken,
        values,
        isDisabled,
        ...plex
      });
    }
  );
}

const mapDispatchToProps = {
  dispatchFetchPlexResources: fetchPlexResources
};

class PlexMachineInputConnector extends Component {

  //
  // Lifecycle
  componentDidMount = () => {
    const {
      accessToken,
      dispatchFetchPlexResources
    } = this.props;

    if (accessToken) {
      dispatchFetchPlexResources({ accessToken });
    }

  };

  componentDidUpdate(prevProps) {
    const {
      accessToken,
      dispatchFetchPlexResources
    } = this.props;

    const oldToken = prevProps.accessToken;
    if (accessToken && accessToken !== oldToken) {
      dispatchFetchPlexResources({ accessToken });
    }
  }

  render() {
    const {
      isFetching,
      isPopulated,
      isDisabled,
      value,
      values,
      onChange
    } = this.props;

    return (
      <PlexMachineInput
        isFetching={isFetching}
        isPopulated={isPopulated}
        isDisabled={isDisabled}
        value={value}
        values={values}
        onChange={onChange}
        {...this.props}
      />
    );
  }
}

PlexMachineInputConnector.propTypes = {
  dispatchFetchPlexResources: PropTypes.func.isRequired,
  name: PropTypes.string.isRequired,
  value: PropTypes.string.isRequired,
  values: PropTypes.arrayOf(PropTypes.object).isRequired,
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  isDisabled: PropTypes.bool.isRequired,
  error: PropTypes.object,
  oAuth: PropTypes.object,
  accessToken: PropTypes.string,
  onChange: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(PlexMachineInputConnector);
