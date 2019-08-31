import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { findDOMNode } from 'react-dom';
import { DragSource, DropTarget } from 'react-dnd';
import classNames from 'classnames';
import { QUALITY_PROFILE_FORMAT_ITEM } from 'Helpers/dragTypes';
import QualityProfileFormatItem from './QualityProfileFormatItem';
import styles from './QualityProfileFormatItemDragSource.css';

const qualityProfileFormatItemDragSource = {
  beginDrag({ formatId, name, allowed, sortIndex }) {
    return {
      formatId,
      name,
      allowed,
      sortIndex
    };
  },

  endDrag(props, monitor, component) {
    props.onQualityProfileFormatItemDragEnd(monitor.getItem(), monitor.didDrop());
  }
};

const qualityProfileFormatItemDropTarget = {
  hover(props, monitor, component) {
    const dragIndex = monitor.getItem().sortIndex;
    const hoverIndex = props.sortIndex;

    const hoverBoundingRect = findDOMNode(component).getBoundingClientRect();
    const hoverMiddleY = (hoverBoundingRect.bottom - hoverBoundingRect.top) / 2;
    const clientOffset = monitor.getClientOffset();
    const hoverClientY = clientOffset.y - hoverBoundingRect.top;

    // Moving up, only trigger if drag position is above 50%
    if (dragIndex < hoverIndex && hoverClientY > hoverMiddleY) {
      return;
    }

    // Moving down, only trigger if drag position is below 50%
    if (dragIndex > hoverIndex && hoverClientY < hoverMiddleY) {
      return;
    }

    props.onQualityProfileFormatItemDragMove(dragIndex, hoverIndex);
  }
};

function collectDragSource(connect, monitor) {
  return {
    connectDragSource: connect.dragSource(),
    isDragging: monitor.isDragging()
  };
}

function collectDropTarget(connect, monitor) {
  return {
    connectDropTarget: connect.dropTarget(),
    isOver: monitor.isOver()
  };
}

class QualityProfileFormatItemDragSource extends Component {

  //
  // Render

  render() {
    const {
      formatId,
      name,
      allowed,
      sortIndex,
      isDragging,
      isDraggingUp,
      isDraggingDown,
      isOver,
      connectDragSource,
      connectDropTarget,
      onQualityProfileFormatItemAllowedChange
    } = this.props;

    const isBefore = !isDragging && isDraggingUp && isOver;
    const isAfter = !isDragging && isDraggingDown && isOver;

    // if (isDragging && !isOver) {
    //   return null;
    // }

    return connectDropTarget(
      <div
        className={classNames(
          styles.qualityProfileFormatItemDragSource,
          isBefore && styles.isDraggingUp,
          isAfter && styles.isDraggingDown
        )}
      >
        {
          isBefore &&
            <div
              className={classNames(
                styles.qualityProfileFormatItemPlaceholder,
                styles.qualityProfileFormatItemPlaceholderBefore
              )}
            />
        }

        <QualityProfileFormatItem
          formatId={formatId}
          name={name}
          allowed={allowed}
          sortIndex={sortIndex}
          isDragging={isDragging}
          isOver={isOver}
          connectDragSource={connectDragSource}
          onQualityProfileFormatItemAllowedChange={onQualityProfileFormatItemAllowedChange}
        />

        {
          isAfter &&
            <div
              className={classNames(
                styles.qualityProfileFormatItemPlaceholder,
                styles.qualityProfileFormatItemPlaceholderAfter
              )}
            />
        }
      </div>
    );
  }
}

QualityProfileFormatItemDragSource.propTypes = {
  formatId: PropTypes.number.isRequired,
  name: PropTypes.string.isRequired,
  allowed: PropTypes.bool.isRequired,
  sortIndex: PropTypes.number.isRequired,
  isDragging: PropTypes.bool,
  isDraggingUp: PropTypes.bool,
  isDraggingDown: PropTypes.bool,
  isOver: PropTypes.bool,
  connectDragSource: PropTypes.func,
  connectDropTarget: PropTypes.func,
  onQualityProfileFormatItemAllowedChange: PropTypes.func.isRequired,
  onQualityProfileFormatItemDragMove: PropTypes.func.isRequired,
  onQualityProfileFormatItemDragEnd: PropTypes.func.isRequired
};

export default DropTarget(
  QUALITY_PROFILE_FORMAT_ITEM,
  qualityProfileFormatItemDropTarget,
  collectDropTarget
)(DragSource(
  QUALITY_PROFILE_FORMAT_ITEM,
  qualityProfileFormatItemDragSource,
  collectDragSource
)(QualityProfileFormatItemDragSource));
