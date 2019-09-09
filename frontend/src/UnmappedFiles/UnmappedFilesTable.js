import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { align, icons, sortDirections } from 'Helpers/Props';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import VirtualTable from 'Components/Table/VirtualTable';
import TableOptionsModalWrapper from 'Components/Table/TableOptions/TableOptionsModalWrapper';
import PageContent from 'Components/Page/PageContent';
import PageContentBodyConnector from 'Components/Page/PageContentBodyConnector';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import UnmappedFilesTableRow from './UnmappedFilesTableRow';
import UnmappedFilesTableHeader from './UnmappedFilesTableHeader';

class UnmappedFilesTable extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      contentBody: null,
      scrollTop: 0
    };
  }

  //
  // Control

  setContentBodyRef = (ref) => {
    this.setState({ contentBody: ref });
  }

  rowRenderer = ({ key, rowIndex, style }) => {
    const {
      items,
      columns,
      deleteUnmappedFile
    } = this.props;

    const item = items[rowIndex];

    return (
      <UnmappedFilesTableRow
        style={style}
        key={key}
        columns={columns}
        deleteUnmappedFile={deleteUnmappedFile}
        {...item}
      />
    );
  }

  //
  // Listeners

  onScroll = ({ scrollTop }) => {
    this.setState({ scrollTop });
  }

  render() {

    const {
      isFetching,
      isPopulated,
      error,
      items,
      columns,
      sortKey,
      sortDirection,
      onTableOptionChange,
      onSortPress,
      deleteUnmappedFile,
      ...otherProps
    } = this.props;

    const {
      scrollTop,
      contentBody
    } = this.state;

    return (
      <PageContent title="UnmappedFiles">
        <PageToolbar>
          <PageToolbarSection alignContent={align.RIGHT}>
            <TableOptionsModalWrapper
              {...otherProps}
              columns={columns}
              onTableOptionChange={onTableOptionChange}
            >
              <PageToolbarButton
                label="Options"
                iconName={icons.TABLE}
              />
            </TableOptionsModalWrapper>

          </PageToolbarSection>
        </PageToolbar>

        <PageContentBodyConnector
          ref={this.setContentBodyRef}
          onScroll={this.onScroll}
        >
          {
            isFetching && !isPopulated &&
              <LoadingIndicator />
          }

          {
            isPopulated && !error && !items.length &&
              <div>
                Success! My work is done, all files on disk are matched to known tracks.
              </div>
          }

          {
            isPopulated && !error && !!items.length && contentBody &&
              <VirtualTable
                items={items}
                columns={columns}
                contentBody={contentBody}
                isSmallScreen={false}
                scrollTop={scrollTop}
                onScroll={this.onScroll}
                overscanRowCount={10}
                rowRenderer={this.rowRenderer}
                header={
                  <UnmappedFilesTableHeader
                    columns={columns}
                    sortKey={sortKey}
                    sortDirection={sortDirection}
                    onTableOptionChange={onTableOptionChange}
                    onSortPress={onSortPress}
                  />
                }
                sortKey={sortKey}
                sortDirection={sortDirection}
              />
          }
        </PageContentBodyConnector>
      </PageContent>
    );
  }
}

UnmappedFilesTable.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  sortKey: PropTypes.string,
  sortDirection: PropTypes.oneOf(sortDirections.all),
  onTableOptionChange: PropTypes.func.isRequired,
  onSortPress: PropTypes.func.isRequired,
  deleteUnmappedFile: PropTypes.func.isRequired
};

export default UnmappedFilesTable;
