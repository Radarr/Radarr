import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { icons } from 'Helpers/Props';
import IconButton from 'Components/Link/IconButton';
import VirtualTableHeader from 'Components/Table/VirtualTableHeader';
import VirtualTableHeaderCell from 'Components/Table/VirtualTableHeaderCell';
import TableOptionsModal from 'Components/Table/TableOptions/TableOptionsModal';
import MovieIndexTableOptionsConnector from './MovieIndexTableOptionsConnector';
import styles from './MovieIndexHeader.css';

class MovieIndexHeader extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isTableOptionsModalOpen: false
    };
  }

  //
  // Listeners

  onTableOptionsPress = () => {
    this.setState({ isTableOptionsModalOpen: true });
  }

  onTableOptionsModalClose = () => {
    this.setState({ isTableOptionsModalOpen: false });
  }

  //
  // Render

  render() {
    const {
      showSearchAction,
      columns,
      onTableOptionChange,
      ...otherProps
    } = this.props;

    return (
      <VirtualTableHeader>
        {
          columns.map((column) => {
            const {
              name,
              label,
              isSortable,
              isVisible
            } = column;

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
                  {...otherProps}
                >
                  <IconButton
                    name={icons.ADVANCED_SETTINGS}
                    onPress={this.onTableOptionsPress}
                  />
                </VirtualTableHeaderCell>
              );
            }

            return (
              <VirtualTableHeaderCell
                key={name}
                className={styles[name]}
                name={name}
                isSortable={isSortable}
                {...otherProps}
              >
                {label}
              </VirtualTableHeaderCell>
            );
          })
        }

        <TableOptionsModal
          isOpen={this.state.isTableOptionsModalOpen}
          columns={columns}
          optionsComponent={MovieIndexTableOptionsConnector}
          onTableOptionChange={onTableOptionChange}
          onModalClose={this.onTableOptionsModalClose}
        />
      </VirtualTableHeader>
    );
  }
}

MovieIndexHeader.propTypes = {
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  onTableOptionChange: PropTypes.func.isRequired
};

export default MovieIndexHeader;
