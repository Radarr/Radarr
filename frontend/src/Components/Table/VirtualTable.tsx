import React, { ReactNode, useCallback, useEffect, useRef } from 'react';
import { Grid, GridCellProps, WindowScroller } from 'react-virtualized';
import ModelBase from 'App/ModelBase';
import Scroller from 'Components/Scroller/Scroller';
import useMeasure from 'Helpers/Hooks/useMeasure';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { scrollDirections } from 'Helpers/Props';
import hasDifferentItemsOrOrder from 'Utilities/Object/hasDifferentItemsOrOrder';
import styles from './VirtualTable.css';

const ROW_HEIGHT = 38;

function overscanIndicesGetter(options: {
  cellCount: number;
  overscanCellsCount: number;
  startIndex: number;
  stopIndex: number;
}) {
  const { cellCount, overscanCellsCount, startIndex, stopIndex } = options;

  // The default getter takes the scroll direction into account,
  // but that can cause issues. Ignore the scroll direction and
  // always over return more items.

  const overscanStartIndex = startIndex - overscanCellsCount;
  const overscanStopIndex = stopIndex + overscanCellsCount;

  return {
    overscanStartIndex: Math.max(0, overscanStartIndex),
    overscanStopIndex: Math.min(cellCount - 1, overscanStopIndex),
  };
}

interface VirtualTableProps<T extends ModelBase> {
  isSmallScreen: boolean;
  className?: string;
  items: T[];
  scrollIndex?: number;
  scrollTop?: number;
  scroller: Element;
  header: React.ReactNode;
  headerHeight?: number;
  rowRenderer: (rowProps: GridCellProps) => ReactNode;
  rowHeight?: number;
}

function VirtualTable<T extends ModelBase>({
  isSmallScreen,
  className = styles.tableContainer,
  items,
  scroller,
  scrollIndex,
  scrollTop,
  header,
  headerHeight = 38,
  rowHeight = ROW_HEIGHT,
  rowRenderer,
  ...otherProps
}: VirtualTableProps<T>) {
  const [measureRef, bounds] = useMeasure();
  const gridRef = useRef<Grid>(null);
  const scrollRestored = useRef(false);
  const previousScrollIndex = usePrevious(scrollIndex);
  const previousItems = usePrevious(items);

  const width = bounds.width;

  const gridStyle = {
    boxSizing: undefined,
    direction: undefined,
    height: undefined,
    position: undefined,
    willChange: undefined,
    overflow: undefined,
    width: undefined,
  };

  const containerStyle = {
    position: undefined,
  };

  const handleScrollToPosition = useCallback(
    ({
      scrollTop = 0,
      scrollLeft = 0,
    }: {
      scrollTop: number;
      scrollLeft: number;
    }) => {
      scroller?.scrollTo({ top: scrollTop, left: scrollLeft });
    },
    [scroller]
  );

  const handleScrollToCell = useCallback(
    ({
      rowIndex = 0,
      columnIndex = 0,
    }: {
      rowIndex: number;
      columnIndex: number;
    }) => {
      if (gridRef.current) {
        const scrollOffset = gridRef.current.getOffsetForCell({
          rowIndex,
          columnIndex,
        });

        handleScrollToPosition(scrollOffset);
      }
    },
    [gridRef, handleScrollToPosition]
  );

  useEffect(() => {
    if (gridRef.current && width > 0) {
      gridRef.current.recomputeGridSize();
    }
  }, [width]);

  useEffect(() => {
    if (
      gridRef.current &&
      previousItems &&
      hasDifferentItemsOrOrder(previousItems, items)
    ) {
      gridRef.current.recomputeGridSize();
    }
  }, [items, previousItems]);

  useEffect(() => {
    if (gridRef.current && scrollTop && !scrollRestored.current) {
      handleScrollToPosition({ scrollLeft: 0, scrollTop });
      scrollRestored.current = true;
    }
  }, [scrollTop, handleScrollToPosition]);

  useEffect(() => {
    if (
      gridRef.current &&
      scrollIndex != null &&
      scrollIndex !== previousScrollIndex
    ) {
      handleScrollToCell({
        rowIndex: scrollIndex,
        columnIndex: 0,
      });
    }
  }, [scrollIndex, previousScrollIndex, handleScrollToCell]);

  return (
    <WindowScroller scrollElement={isSmallScreen ? undefined : scroller}>
      {({ height, registerChild, onChildScroll, scrollTop }) => {
        if (!height) {
          return null;
        }
        return (
          <div ref={measureRef}>
            <Scroller
              className={className}
              scrollDirection={scrollDirections.HORIZONTAL}
            >
              {header}

              <div ref={registerChild}>
                <Grid
                  {...otherProps}
                  ref={gridRef}
                  autoContainerWidth={true}
                  autoHeight={true}
                  autoWidth={true}
                  width={width}
                  height={height}
                  headerHeight={height - headerHeight}
                  rowHeight={rowHeight}
                  rowCount={items.length}
                  columnCount={1}
                  columnWidth={width}
                  scrollTop={scrollTop}
                  overscanRowCount={2}
                  cellRenderer={rowRenderer}
                  overscanIndicesGetter={overscanIndicesGetter}
                  scrollToAlignment="start"
                  isScrollingOptout={true}
                  className={styles.tableBodyContainer}
                  style={gridStyle}
                  containerStyle={containerStyle}
                  onScroll={onChildScroll}
                />
              </div>
            </Scroller>
          </div>
        );
      }}
    </WindowScroller>
  );
}

export default VirtualTable;
