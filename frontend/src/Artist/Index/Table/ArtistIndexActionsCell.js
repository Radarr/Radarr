import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { icons } from 'Helpers/Props';
import IconButton from 'Components/Link/IconButton';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import VirtualTableRowCell from 'Components/Table/Cells/VirtualTableRowCell';
import EditArtistModalConnector from 'Artist/Edit/EditArtistModalConnector';
import DeleteArtistModal from 'Artist/Delete/DeleteArtistModal';

class ArtistIndexActionsCell extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isEditArtistModalOpen: false,
      isDeleteArtistModalOpen: false
    };
  }

  //
  // Listeners

  onEditArtistPress = () => {
    this.setState({ isEditArtistModalOpen: true });
  }

  onEditArtistModalClose = () => {
    this.setState({ isEditArtistModalOpen: false });
  }

  onDeleteArtistPress = () => {
    this.setState({
      isEditArtistModalOpen: false,
      isDeleteArtistModalOpen: true
    });
  }

  onDeleteArtistModalClose = () => {
    this.setState({ isDeleteArtistModalOpen: false });
  }

  //
  // Render

  render() {
    const {
      id,
      isRefreshingArtist,
      onRefreshArtistPress,
      ...otherProps
    } = this.props;

    const {
      isEditArtistModalOpen,
      isDeleteArtistModalOpen
    } = this.state;

    return (
      <VirtualTableRowCell
        {...otherProps}
      >
        <SpinnerIconButton
          name={icons.REFRESH}
          title="Refresh Artist"
          isSpinning={isRefreshingArtist}
          onPress={onRefreshArtistPress}
        />

        <IconButton
          name={icons.EDIT}
          title="Edit Artist"
          onPress={this.onEditArtistPress}
        />

        <EditArtistModalConnector
          isOpen={isEditArtistModalOpen}
          artistId={id}
          onModalClose={this.onEditArtistModalClose}
          onDeleteArtistPress={this.onDeleteArtistPress}
        />

        <DeleteArtistModal
          isOpen={isDeleteArtistModalOpen}
          artistId={id}
          onModalClose={this.onDeleteArtistModalClose}
        />
      </VirtualTableRowCell>
    );
  }
}

ArtistIndexActionsCell.propTypes = {
  id: PropTypes.number.isRequired,
  isRefreshingArtist: PropTypes.bool.isRequired,
  onRefreshArtistPress: PropTypes.func.isRequired
};

export default ArtistIndexActionsCell;
