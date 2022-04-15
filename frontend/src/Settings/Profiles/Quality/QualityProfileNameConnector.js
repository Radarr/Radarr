import PropTypes from 'prop-types';
import React from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';

function createMapStateToProps() {
  return createSelector(
    (state, { qualityProfileIds }) => qualityProfileIds,
    (state) => state.settings.qualityProfiles.items,
    (qualityProfileIds, allProfiles) => {
      let name = 'Multiple';

      if (qualityProfileIds.length === 1) {
        const profile = allProfiles.find((p) => {
          return p.id === qualityProfileIds[0];
        });

        if (profile) {
          name = profile.name;
        }
      }

      return {
        name
      };
    }
  );
}

function QualityProfileNameConnector({ name, ...otherProps }) {
  return (
    <span>
      {name}
    </span>
  );
}

QualityProfileNameConnector.propTypes = {
  qualityProfileIds: PropTypes.arrayOf(PropTypes.number).isRequired,
  name: PropTypes.string.isRequired
};

export default connect(createMapStateToProps)(QualityProfileNameConnector);
