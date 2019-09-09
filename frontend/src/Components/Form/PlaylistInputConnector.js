import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchOptions, clearOptions } from 'Store/Actions/providerOptionActions';
import PlaylistInput from './PlaylistInput';

function createMapStateToProps() {
  return createSelector(
    (state) => state.providerOptions,
    (state) => {
      const {
        items,
        ...otherState
      } = state;
      return ({
        user: items.user ? items.user : '',
        items: items.playlists ? items.playlists : [],
        ...otherState
      });
    }
  );
}

const mapDispatchToProps = {
  dispatchFetchOptions: fetchOptions,
  dispatchClearOptions: clearOptions
};

class PlaylistInputConnector extends Component {

  //
  // Lifecycle

  componentDidMount = () => {
    if (this._getAccessToken(this.props)) {
      this._populate();
    }
  }

  componentDidUpdate(prevProps, prevState) {
    const newToken = this._getAccessToken(this.props);
    const oldToken = this._getAccessToken(prevProps);
    if (newToken && newToken !== oldToken) {
      this._populate();
    }
  }

  componentWillUnmount = () => {
    this.props.dispatchClearOptions();
  }

  //
  // Control

  _populate() {
    const {
      provider,
      providerData,
      dispatchFetchOptions
    } = this.props;

    dispatchFetchOptions({
      action: 'getPlaylists',
      provider,
      providerData
    });
  }

  _getAccessToken(props) {
    return _.filter(props.providerData.fields, { name: 'accessToken' })[0].value;
  }

  //
  // Render

  render() {
    return (
      <PlaylistInput
        {...this.props}
        onRefreshPress={this.onRefreshPress}
      />
    );
  }
}

PlaylistInputConnector.propTypes = {
  provider: PropTypes.string.isRequired,
  providerData: PropTypes.object.isRequired,
  name: PropTypes.string.isRequired,
  onChange: PropTypes.func.isRequired,
  dispatchFetchOptions: PropTypes.func.isRequired,
  dispatchClearOptions: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(PlaylistInputConnector);
