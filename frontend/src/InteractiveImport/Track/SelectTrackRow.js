import PropTypes from 'prop-types';
import React, { Component } from 'react';
import TableRowButton from 'Components/Table/TableRowButton';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableSelectCell from 'Components/Table/Cells/TableSelectCell';
import { icons, kinds, tooltipPositions } from 'Helpers/Props';
import Icon from 'Components/Icon';
import Popover from 'Components/Tooltip/Popover';

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
      hasFile,
      importSelected,
      isSelected,
      isDisabled,
      onSelectedChange
    } = this.props;

    let iconName = icons.UNKNOWN;
    let iconKind = kinds.DEFAULT;
    let iconTip = '';

    if (hasFile && !importSelected) {
      iconName = icons.DOWNLOADED;
      iconKind = kinds.DEFAULT;
      iconTip = 'Track already in library.';
    } else if (!hasFile && !importSelected) {
      iconName = icons.UNKNOWN;
      iconKind = kinds.DEFAULT;
      iconTip = 'Track missing from library and no import selected.';
    } else if (importSelected && hasFile) {
      iconName = icons.FILEIMPORT;
      iconKind = kinds.WARNING;
      iconTip = 'Warning: Existing track will be replaced by download.';
    } else if (importSelected && !hasFile) {
      iconName = icons.FILEIMPORT;
      iconKind = kinds.DEFAULT;
      iconTip = 'Track missing from library and selected for import.';
    }

    // isDisabled can only be true if importSelected is true
    if (isDisabled) {
      iconTip = `${iconTip}\nAnother file is selected to import for this track.`;
    }

    return (
      <TableRowButton
        onPress={this.onPress}
        isDisabled={isDisabled}
      >
        <TableSelectCell
          id={id}
          isSelected={isSelected}
          onSelectedChange={onSelectedChange}
          isDisabled={isDisabled}
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

        <TableRowCell>
          <Popover
            anchor={
              <Icon
                name={iconName}
                kind={iconKind}
              />
            }
            title={'Track status'}
            body={iconTip}
            position={tooltipPositions.LEFT}
          />
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
  hasFile: PropTypes.bool.isRequired,
  importSelected: PropTypes.bool.isRequired,
  isSelected: PropTypes.bool,
  isDisabled: PropTypes.bool,
  onSelectedChange: PropTypes.func.isRequired
};

export default SelectTrackRow;
