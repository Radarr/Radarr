import _ from 'lodash';
import React from 'react';
import { useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import { kinds } from 'Helpers/Props';
import Label from './Label';
import styles from './QualityProfileList.css';

interface QualityProfileListProps {
  qualityProfileIds: number[];
}

function QualityProfileList(props: QualityProfileListProps) {
  const { qualityProfileIds } = props;
  const { qualityProfileList } = useSelector(
    createSelector(
      (state: AppState) => state.settings.qualityProfiles.items,
      (qualityProfileList) => {
        return {
          qualityProfileList,
        };
      }
    )
  );

  return (
    <div className={styles.tags}>
      {qualityProfileIds.map((t) => {
        const qualityProfile = _.find(qualityProfileList, { id: t });

        if (!qualityProfile) {
          return null;
        }

        return (
          <Label key={qualityProfile.id} kind={kinds.INFO}>
            {qualityProfile.name}
          </Label>
        );
      })}
    </div>
  );
}

export default QualityProfileList;
