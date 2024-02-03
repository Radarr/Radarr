import _ from 'lodash';
import PropTypes from 'prop-types';
import React from 'react';
import { kinds, sizes } from 'Helpers/Props';
import Label from './Label';
import styles from './ImportListList.css';

function ImportListList({ lists, importListList }) {
  return (
    <div className={styles.lists}>
      {
        lists.map((t) => {
          const list = _.find(importListList, { id: t });

          if (!list) {
            return null;
          }

          return (
            <Label
              key={list.id}
              kind={kinds.SUCCESS}
              size={sizes.MEDIUM}
            >
              {list.name}
            </Label>
          );
        })
      }
    </div>
  );
}

ImportListList.propTypes = {
  lists: PropTypes.arrayOf(PropTypes.number).isRequired,
  importListList: PropTypes.arrayOf(PropTypes.object).isRequired
};

ImportListList.defaultProps = {
  lists: []
};

export default ImportListList;
