import PropTypes from 'prop-types';
import React, { Component } from 'react';
import classNames from 'classnames';
import { icons } from 'Helpers/Props';
import Icon from 'Components/Icon';
import CheckInput from 'Components/Form/CheckInput';
import styles from './QualityProfileFormatItem.css';

class QualityProfileFormatItem extends Component {

  //
  // Listeners

  onAllowedChange = ({ value }) => {
    const {
      formatId,
      onQualityProfileFormatItemAllowedChange
    } = this.props;

    onQualityProfileFormatItemAllowedChange(formatId, value);
  }

  //
  // Render

  render() {
    const {
      name,
      allowed,
      isDragging,
      connectDragSource
    } = this.props;

    return (
      <div
        className={classNames(
          styles.qualityProfileFormatItem,
          isDragging && styles.isDragging
        )}
      >
        <label
          className={styles.formatName}
        >
          <CheckInput
            containerClassName={styles.checkContainer}
            name={name}
            value={allowed}
            onChange={this.onAllowedChange}
          />
          {name}
        </label>

        {
          connectDragSource(
            <div className={styles.dragHandle}>
              <Icon
                className={styles.dragIcon}
                name={icons.REORDER}
              />
            </div>
          )
        }
      </div>
    );
  }
}

QualityProfileFormatItem.propTypes = {
  formatId: PropTypes.number.isRequired,
  name: PropTypes.string.isRequired,
  allowed: PropTypes.bool.isRequired,
  sortIndex: PropTypes.number.isRequired,
  isDragging: PropTypes.bool.isRequired,
  connectDragSource: PropTypes.func,
  onQualityProfileFormatItemAllowedChange: PropTypes.func
};

QualityProfileFormatItem.defaultProps = {
  // The drag preview will not connect the drag handle.
  connectDragSource: (node) => node
};

export default QualityProfileFormatItem;
