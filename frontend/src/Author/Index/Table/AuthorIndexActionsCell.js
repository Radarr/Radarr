import PropTypes from 'prop-types';
import React, { Component } from 'react';
import DeleteAuthorModal from 'Author/Delete/DeleteAuthorModal';
import EditAuthorModalConnector from 'Author/Edit/EditAuthorModalConnector';
import IconButton from 'Components/Link/IconButton';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import VirtualTableRowCell from 'Components/Table/Cells/VirtualTableRowCell';
import { icons } from 'Helpers/Props';

class AuthorIndexActionsCell extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isEditAuthorModalOpen: false,
      isDeleteAuthorModalOpen: false
    };
  }

  //
  // Listeners

  onEditAuthorPress = () => {
    this.setState({ isEditAuthorModalOpen: true });
  }

  onEditAuthorModalClose = () => {
    this.setState({ isEditAuthorModalOpen: false });
  }

  onDeleteAuthorPress = () => {
    this.setState({
      isEditAuthorModalOpen: false,
      isDeleteAuthorModalOpen: true
    });
  }

  onDeleteAuthorModalClose = () => {
    this.setState({ isDeleteAuthorModalOpen: false });
  }

  //
  // Render

  render() {
    const {
      id,
      isRefreshingAuthor,
      onRefreshAuthorPress,
      ...otherProps
    } = this.props;

    const {
      isEditAuthorModalOpen,
      isDeleteAuthorModalOpen
    } = this.state;

    return (
      <VirtualTableRowCell
        {...otherProps}
      >
        <SpinnerIconButton
          name={icons.REFRESH}
          title="Refresh Author"
          isSpinning={isRefreshingAuthor}
          onPress={onRefreshAuthorPress}
        />

        <IconButton
          name={icons.EDIT}
          title="Edit Author"
          onPress={this.onEditAuthorPress}
        />

        <EditAuthorModalConnector
          isOpen={isEditAuthorModalOpen}
          authorId={id}
          onModalClose={this.onEditAuthorModalClose}
          onDeleteAuthorPress={this.onDeleteAuthorPress}
        />

        <DeleteAuthorModal
          isOpen={isDeleteAuthorModalOpen}
          authorId={id}
          onModalClose={this.onDeleteAuthorModalClose}
        />
      </VirtualTableRowCell>
    );
  }
}

AuthorIndexActionsCell.propTypes = {
  id: PropTypes.number.isRequired,
  isRefreshingAuthor: PropTypes.bool.isRequired,
  onRefreshAuthorPress: PropTypes.func.isRequired
};

export default AuthorIndexActionsCell;
