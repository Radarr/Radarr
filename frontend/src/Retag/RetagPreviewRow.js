import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { icons } from 'Helpers/Props';
import Icon from 'Components/Icon';
import CheckInput from 'Components/Form/CheckInput';
import styles from './RetagPreviewRow.css';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';

function formatMissing(value) {
  if (value === undefined || value === 0 || value === '0') {
    return (<Icon name={icons.BAN} size={12} />);
  }
  return value;
}

function formatChange(oldValue, newValue) {
  return (
    <div> {formatMissing(oldValue)} <Icon name={icons.ARROW_RIGHT_NO_CIRCLE} size={12} /> {formatMissing(newValue)} </div>
  );
}

class RetagPreviewRow extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      id,
      onSelectedChange
    } = this.props;

    onSelectedChange({ id, value: true });
  }

  //
  // Listeners

  onSelectedChange = ({ value, shiftKey }) => {
    const {
      id,
      onSelectedChange
    } = this.props;

    onSelectedChange({ id, value, shiftKey });
  }

  //
  // Render

  render() {
    const {
      id,
      path,
      changes,
      isSelected
    } = this.props;

    return (
      <div className={styles.row}>
        <CheckInput
          containerClassName={styles.selectedContainer}
          name={id.toString()}
          value={isSelected}
          onChange={this.onSelectedChange}
        />

        <div className={styles.column}>
          <span className={styles.path}>
            {path}
          </span>

          <DescriptionList>
            {
              changes.map(({ field, oldValue, newValue }) => {
                return (
                  <DescriptionListItem
                    key={field}
                    title={field}
                    data={formatChange(oldValue, newValue)}
                  />
                );
              })
            }
          </DescriptionList>
        </div>
      </div>
    );
  }
}

RetagPreviewRow.propTypes = {
  id: PropTypes.number.isRequired,
  path: PropTypes.string.isRequired,
  changes: PropTypes.arrayOf(PropTypes.object).isRequired,
  isSelected: PropTypes.bool,
  onSelectedChange: PropTypes.func.isRequired
};

export default RetagPreviewRow;
