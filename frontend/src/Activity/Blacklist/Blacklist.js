import PropTypes from 'prop-types';
import React, { Component } from 'react';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBodyConnector from 'Components/Page/PageContentBodyConnector';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import TableOptionsModalWrapper from 'Components/Table/TableOptions/TableOptionsModalWrapper';
import TablePager from 'Components/Table/TablePager';
import { align, icons } from 'Helpers/Props';
import BlacklistRowConnector from './BlacklistRowConnector';

class Blacklist extends Component {

  //
  // Render

  render() {
    const {
      isFetching,
      isPopulated,
      isAuthorFetching,
      isAuthorPopulated,
      error,
      items,
      columns,
      totalRecords,
      isClearingBlacklistExecuting,
      onClearBlacklistPress,
      ...otherProps
    } = this.props;

    const isAllPopulated = isPopulated && isAuthorPopulated;
    const isAnyFetching = isFetching || isAuthorFetching;

    return (
      <PageContent title="Blacklist">
        <PageToolbar>
          <PageToolbarSection>
            <PageToolbarButton
              label="Clear"
              iconName={icons.CLEAR}
              isSpinning={isClearingBlacklistExecuting}
              onPress={onClearBlacklistPress}
            />
          </PageToolbarSection>

          <PageToolbarSection alignContent={align.RIGHT}>
            <TableOptionsModalWrapper
              {...otherProps}
              columns={columns}
            >
              <PageToolbarButton
                label="Options"
                iconName={icons.TABLE}
              />
            </TableOptionsModalWrapper>
          </PageToolbarSection>
        </PageToolbar>

        <PageContentBodyConnector>
          {
            isAnyFetching && !isAllPopulated &&
              <LoadingIndicator />
          }

          {
            !isAnyFetching && !!error &&
              <div>Unable to load blacklist</div>
          }

          {
            isAllPopulated && !error && !items.length &&
              <div>
                No history blacklist
              </div>
          }

          {
            isAllPopulated && !error && !!items.length &&
              <div>
                <Table
                  columns={columns}
                  {...otherProps}
                >
                  <TableBody>
                    {
                      items.map((item) => {
                        return (
                          <BlacklistRowConnector
                            key={item.id}
                            columns={columns}
                            {...item}
                          />
                        );
                      })
                    }
                  </TableBody>
                </Table>

                <TablePager
                  totalRecords={totalRecords}
                  isFetching={isFetching}
                  {...otherProps}
                />
              </div>
          }
        </PageContentBodyConnector>
      </PageContent>
    );
  }
}

Blacklist.propTypes = {
  isAuthorFetching: PropTypes.bool.isRequired,
  isAuthorPopulated: PropTypes.bool.isRequired,
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  totalRecords: PropTypes.number,
  isClearingBlacklistExecuting: PropTypes.bool.isRequired,
  onClearBlacklistPress: PropTypes.func.isRequired
};

export default Blacklist;
