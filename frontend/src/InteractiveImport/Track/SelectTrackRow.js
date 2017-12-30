import PropTypes from 'prop-types';
import React, { Component } from 'react';
import TableRowButton from 'Components/Table/TableRowButton';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableSelectCell from 'Components/Table/Cells/TableSelectCell';

class SelectTrackRow extends Component {

  //
  // Listeners

  onPress = () => {
    const {
      id,
      isSelected
    } = this.props;

    this.props.onSelectedChange({ id, value: !isSelected });
  }

  //
  // Render

  render() {
    const {
      id,
      mediumNumber,
      trackNumber,
      title,
      isSelected,
      onSelectedChange
    } = this.props;

    return (
      <TableRowButton onPress={this.onPress}>
        <TableSelectCell
          id={id}
          isSelected={isSelected}
          onSelectedChange={onSelectedChange}
        />

        <TableRowCell>
          {mediumNumber}
        </TableRowCell>

        <TableRowCell>
          {trackNumber}
        </TableRowCell>

        <TableRowCell>
          {title}
        </TableRowCell>

      </TableRowButton>
    );
  }
}

SelectTrackRow.propTypes = {
  id: PropTypes.number.isRequired,
  mediumNumber: PropTypes.number.isRequired,
  trackNumber: PropTypes.number.isRequired,
  title: PropTypes.string.isRequired,
  isSelected: PropTypes.bool,
  onSelectedChange: PropTypes.func.isRequired
};

export default SelectTrackRow;
