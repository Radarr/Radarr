import _ from 'lodash';
import PropTypes from 'prop-types';
import React from 'react';
import { kinds } from 'Helpers/Props';
import Label from './Label';
import styles from './QualityProfileList.css';

function QualityProfileList({ qualityProfileIds, qualityProfileList }) {
  return (
    <div className={styles.tags}>
      {
        qualityProfileIds.map((t) => {
          const qualityProfile = _.find(qualityProfileList, { id: t });

          if (!qualityProfile) {
            return null;
          }

          return (
            <Label
              key={qualityProfile.id}
              kind={kinds.INFO}
            >
              {qualityProfile.name}
            </Label>
          );
        })
      }
    </div>
  );
}

QualityProfileList.propTypes = {
  qualityProfileIds: PropTypes.arrayOf(PropTypes.number).isRequired,
  qualityProfileList: PropTypes.arrayOf(PropTypes.object).isRequired
};

export default QualityProfileList;
