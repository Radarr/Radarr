import classNames from 'classnames';
import React, { useCallback } from 'react';
import { useDispatch } from 'react-redux';
import { useSelect } from 'App/SelectContext';
import IconButton from 'Components/Link/IconButton';
import Column from 'Components/Table/Column';
import TableOptionsModalWrapper from 'Components/Table/TableOptions/TableOptionsModalWrapper';
import VirtualTableHeader from 'Components/Table/VirtualTableHeader';
import VirtualTableHeaderCell from 'Components/Table/VirtualTableHeaderCell';
import VirtualTableSelectAllHeaderCell from 'Components/Table/VirtualTableSelectAllHeaderCell';
import { icons } from 'Helpers/Props';
import SortDirection from 'Helpers/Props/SortDirection';
import {
  setMovieSort,
  setMovieTableOption,
} from 'Store/Actions/movieIndexActions';
import { CheckInputChanged } from 'typings/inputs';
import MovieIndexTableOptions from './MovieIndexTableOptions';
import styles from './MovieIndexTableHeader.css';

interface MovieIndexTableHeaderProps {
  columns: Column[];
  sortKey?: string;
  sortDirection?: SortDirection;
  isSelectMode: boolean;
}

function MovieIndexTableHeader(props: MovieIndexTableHeaderProps) {
  const { columns, sortKey, sortDirection, isSelectMode } = props;
  const dispatch = useDispatch();
  const [selectState, selectDispatch] = useSelect();

  const onSortPress = useCallback(
    (value: string) => {
      dispatch(setMovieSort({ sortKey: value }));
    },
    [dispatch]
  );

  const onTableOptionChange = useCallback(
    (payload: unknown) => {
      dispatch(setMovieTableOption(payload));
    },
    [dispatch]
  );

  const onSelectAllChange = useCallback(
    ({ value }: CheckInputChanged) => {
      selectDispatch({
        type: value ? 'selectAll' : 'unselectAll',
      });
    },
    [selectDispatch]
  );

  return (
    <VirtualTableHeader>
      {isSelectMode ? (
        <VirtualTableSelectAllHeaderCell
          allSelected={selectState.allSelected}
          allUnselected={selectState.allUnselected}
          onSelectAllChange={onSelectAllChange}
        />
      ) : null}

      {columns.map((column) => {
        const { name, label, isSortable, isVisible } = column;

        if (!isVisible) {
          return null;
        }

        if (name === 'actions') {
          return (
            <VirtualTableHeaderCell
              key={name}
              className={styles[name]}
              name={name}
              isSortable={false}
            >
              <TableOptionsModalWrapper
                columns={columns}
                optionsComponent={MovieIndexTableOptions}
                onTableOptionChange={onTableOptionChange}
              >
                <IconButton name={icons.ADVANCED_SETTINGS} />
              </TableOptionsModalWrapper>
            </VirtualTableHeaderCell>
          );
        }

        return (
          <VirtualTableHeaderCell
            key={name}
            className={classNames(
              // eslint-disable-next-line @typescript-eslint/ban-ts-comment
              // @ts-ignore
              styles[name]
            )}
            name={name}
            sortKey={sortKey}
            sortDirection={sortDirection}
            isSortable={isSortable}
            onSortPress={onSortPress}
          >
            {label}
          </VirtualTableHeaderCell>
        );
      })}
    </VirtualTableHeader>
  );
}

export default MovieIndexTableHeader;
