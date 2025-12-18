import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { DragLayer } from 'react-dnd';
import DragPreviewLayer from 'Components/DragPreviewLayer';
import { TABLE_COLUMN } from 'Helpers/dragTypes';
import dimensions from 'Styles/Variables/dimensions.js';
import TableOptionsColumn from './TableOptionsColumn';
import styles from './TableOptionsColumnDragPreview.css';

const formGroupSmallWidth = Number.Number.parseInt(dimensions.formGroupSmallWidth);
const formLabelLargeWidth = Number.Number.parseInt(dimensions.formLabelLargeWidth);
const formLabelRightMarginWidth = Number.Number.parseInt(dimensions.formLabelRightMarginWidth);
const dragHandleWidth = Number.Number.parseInt(dimensions.dragHandleWidth);

function collectDragLayer(monitor) {
  return {
    item: monitor.getItem(),
    itemType: monitor.getItemType(),
    currentOffset: monitor.getSourceClientOffset()
  };
}

class TableOptionsColumnDragPreview extends Component {

  //
  // Render

  render() {
    const {
      item,
      itemType,
      currentOffset
    } = this.props;

    if (!currentOffset || itemType !== TABLE_COLUMN) {
      return null;
    }

    // The offset is shifted because the drag handle is on the right edge of the
    // list item and the preview is wider than the drag handle.

    const { x, y } = currentOffset;
    const handleOffset = formGroupSmallWidth - formLabelLargeWidth - formLabelRightMarginWidth - dragHandleWidth;
    const transform = `translate3d(${x - handleOffset}px, ${y}px, 0)`;

    const style = {
      position: 'absolute',
      WebkitTransform: transform,
      msTransform: transform,
      transform
    };

    return (
      <DragPreviewLayer>
        <div
          className={styles.dragPreview}
          style={style}
        >
          <TableOptionsColumn
            isDragging={false}
            {...item}
          />
        </div>
      </DragPreviewLayer>
    );
  }
}

TableOptionsColumnDragPreview.propTypes = {
  item: PropTypes.object,
  itemType: PropTypes.string,
  currentOffset: PropTypes.shape({
    x: PropTypes.number.isRequired,
    y: PropTypes.number.isRequired
  })
};

export default DragLayer(collectDragLayer)(TableOptionsColumnDragPreview);
