import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { icons } from 'Helpers/Props';
import IconButton from 'Components/Link/IconButton';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import AlbumDetailsModal from './AlbumDetailsModal';
import EditAlbumModalConnector from './Edit/EditAlbumModalConnector';
import styles from './AlbumSearchCell.css';

class AlbumSearchCell extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isDetailsModalOpen: false,
      isEditAlbumModalOpen: false
    };
  }

  //
  // Listeners

  onManualSearchPress = () => {
    this.setState({ isDetailsModalOpen: true });
  }

  onDetailsModalClose = () => {
    this.setState({ isDetailsModalOpen: false });
  }

  onEditAlbumPress = () => {
    this.setState({ isEditAlbumModalOpen: true });
  }

  onEditAlbumModalClose = () => {
    this.setState({ isEditAlbumModalOpen: false });
  }

  //
  // Render

  render() {
    const {
      albumId,
      artistId,
      albumTitle,
      isSearching,
      onSearchPress,
      ...otherProps
    } = this.props;

    return (
      <TableRowCell className={styles.AlbumSearchCell}>
        <SpinnerIconButton
          name={icons.SEARCH}
          isSpinning={isSearching}
          onPress={onSearchPress}
        />

        <IconButton
          name={icons.INTERACTIVE}
          onPress={this.onManualSearchPress}
        />

        <IconButton
          name={icons.EDIT}
          title="Edit Album"
          onPress={this.onEditAlbumPress}
        />

        <AlbumDetailsModal
          isOpen={this.state.isDetailsModalOpen}
          albumId={albumId}
          artistId={artistId}
          albumTitle={albumTitle}
          selectedTab="search"
          startInteractiveSearch={true}
          onModalClose={this.onDetailsModalClose}
          {...otherProps}
        />

        <EditAlbumModalConnector
          isOpen={this.state.isEditAlbumModalOpen}
          albumId={albumId}
          artistId={artistId}
          onModalClose={this.onEditAlbumModalClose}
        />
      </TableRowCell>
    );
  }
}

AlbumSearchCell.propTypes = {
  albumId: PropTypes.number.isRequired,
  artistId: PropTypes.number.isRequired,
  albumTitle: PropTypes.string.isRequired,
  isSearching: PropTypes.bool.isRequired,
  onSearchPress: PropTypes.func.isRequired
};

export default AlbumSearchCell;
