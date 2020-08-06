import _ from 'lodash';
import PropTypes from 'prop-types';
import React from 'react';
import { kinds } from 'Helpers/Props';
import Label from './Label';
import styles from './NetImportList.css';

function NetImportList({ lists, netImportList }) {
  return (
    <div className={styles.lists}>
      {
        lists.map((t) => {
          const list = _.find(netImportList, { id: t });

          if (!list) {
            return null;
          }

          return (
            <Label
              key={list.id}
              kind={kinds.INFO}
            >
              {list.name}
            </Label>
          );
        })
      }
    </div>
  );
}

NetImportList.propTypes = {
  lists: PropTypes.arrayOf(PropTypes.number).isRequired,
  netImportList: PropTypes.arrayOf(PropTypes.object).isRequired
};

export default NetImportList;
