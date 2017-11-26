import PropTypes from 'prop-types';
import React, { Component } from 'react';
import classNames from 'classnames';
import CheckInput from 'Components/Form/CheckInput';
import styles from './TypeItem.css';

class PrimaryTypeItem extends Component {

  //
  // Listeners

  onAllowedChange = ({ value }) => {
    const {
      albumTypeId,
      onMetadataPrimaryTypeItemAllowedChange
    } = this.props;

    onMetadataPrimaryTypeItemAllowedChange(albumTypeId, value);
  }

  //
  // Render

  render() {
    const {
      name,
      allowed
    } = this.props;

    return (
      <div
        className={classNames(
          styles.metadataProfileItem
        )}
      >
        <label
          className={styles.albumTypeName}
        >
          <CheckInput
            containerClassName={styles.checkContainer}
            name={name}
            value={allowed}
            onChange={this.onAllowedChange}
          />
          {name}
        </label>
      </div>
    );
  }
}

PrimaryTypeItem.propTypes = {
  albumTypeId: PropTypes.number.isRequired,
  name: PropTypes.string.isRequired,
  allowed: PropTypes.bool.isRequired,
  sortIndex: PropTypes.number.isRequired,
  onMetadataPrimaryTypeItemAllowedChange: PropTypes.func
};

export default PrimaryTypeItem;
