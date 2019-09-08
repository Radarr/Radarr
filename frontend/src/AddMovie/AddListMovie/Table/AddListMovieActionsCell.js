import PropTypes from 'prop-types';
import React, { Component } from 'react';
import VirtualTableRowCell from 'Components/Table/Cells/VirtualTableRowCell';

class AddListMovieActionsCell extends Component {

  //
  // Render

  render() {
    const {
      id,
      ...otherProps
    } = this.props;

    return (
      <VirtualTableRowCell
        {...otherProps}
      >
        {/* <SpinnerIconButton
          name={icons.REFRESH}
          title="Refresh Movie"
          isSpinning={isRefreshingMovie}
          onPress={onRefreshMoviePress}
        />

        <IconButton
          name={icons.EDIT}
          title="Edit Movie"
          onPress={this.onEditMoviePress}
        /> */}

        {/* <EditMovieModalConnector
          isOpen={isEditMovieModalOpen}
          movieId={id}
          onModalClose={this.onEditMovieModalClose}
          onDeleteMoviePress={this.onDeleteMoviePress}
        />

        <DeleteMovieModal
          isOpen={isDeleteMovieModalOpen}
          movieId={id}
          onModalClose={this.onDeleteMovieModalClose}
        /> */}
      </VirtualTableRowCell>
    );
  }
}

AddListMovieActionsCell.propTypes = {
  id: PropTypes.number.isRequired
};

export default AddListMovieActionsCell;
