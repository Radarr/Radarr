import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { icons } from 'Helpers/Props';
import IconButton from 'Components/Link/IconButton';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import VirtualTableRowCell from 'Components/Table/Cells/VirtualTableRowCell';
import EditMovieModalConnector from 'Movie/Edit/EditMovieModalConnector';
import DeleteMovieModal from 'Movie/Delete/DeleteMovieModal';

class MovieIndexActionsCell extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isEditMovieModalOpen: false,
      isDeleteMovieModalOpen: false
    };
  }

  //
  // Listeners

  onEditMoviePress = () => {
    this.setState({ isEditMovieModalOpen: true });
  }

  onEditMovieModalClose = () => {
    this.setState({ isEditMovieModalOpen: false });
  }

  onDeleteMoviePress = () => {
    this.setState({
      isEditMovieModalOpen: false,
      isDeleteMovieModalOpen: true
    });
  }

  onDeleteMovieModalClose = () => {
    this.setState({ isDeleteMovieModalOpen: false });
  }

  //
  // Render

  render() {
    const {
      id,
      isRefreshingMovie,
      onRefreshMoviePress,
      ...otherProps
    } = this.props;

    const {
      isEditMovieModalOpen,
      isDeleteMovieModalOpen
    } = this.state;

    return (
      <VirtualTableRowCell
        {...otherProps}
      >
        <SpinnerIconButton
          name={icons.REFRESH}
          title="Refresh Movie"
          isSpinning={isRefreshingMovie}
          onPress={onRefreshMoviePress}
        />

        <IconButton
          name={icons.EDIT}
          title="Edit Movie"
          onPress={this.onEditMoviePress}
        />

        <EditMovieModalConnector
          isOpen={isEditMovieModalOpen}
          movieId={id}
          onModalClose={this.onEditMovieModalClose}
          onDeleteMoviePress={this.onDeleteMoviePress}
        />

        <DeleteMovieModal
          isOpen={isDeleteMovieModalOpen}
          movieId={id}
          onModalClose={this.onDeleteMovieModalClose}
        />
      </VirtualTableRowCell>
    );
  }
}

MovieIndexActionsCell.propTypes = {
  id: PropTypes.number.isRequired,
  isRefreshingMovie: PropTypes.bool.isRequired,
  onRefreshMoviePress: PropTypes.func.isRequired
};

export default MovieIndexActionsCell;
