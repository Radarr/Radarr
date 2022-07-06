import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableSelectCell from 'Components/Table/Cells/TableSelectCell';
import TableRow from 'Components/Table/TableRow';
import { icons, kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import EditImportListExclusionModalConnector from './EditImportListExclusionModalConnector';
import styles from './ImportListExclusion.css';
import IconButton from 'Components/Link/IconButton';

class ImportListExclusion extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isEditImportExclusionModalOpen: false,
      isDeleteImportExclusionModalOpen: false
    };
  }

  //
  // Listeners

  onEditImportExclusionPress = () => {
    this.setState({ isEditImportExclusionModalOpen: true });
  };

  onEditImportExclusionModalClose = () => {
    this.setState({ isEditImportExclusionModalOpen: false });
  };

  onDeleteImportExclusionPress = () => {
    this.setState({
      isEditImportExclusionModalOpen: false,
      isDeleteImportExclusionModalOpen: true
    });
  };

  onDeleteImportExclusionModalClose = () => {
    this.setState({ isDeleteImportExclusionModalOpen: false });
  };

  onConfirmDeleteImportExclusion = () => {
    this.props.onConfirmDeleteImportExclusion(this.props.id);
  };

  //
  // Render

  render() {
    const {
      id,
      isSelected,
      onSelectedChange,
      columns,
      movieTitle,
      tmdbId,
      movieYear
    } = this.props;

    return (
      <TableRow>
        <TableSelectCell
          id={id}
          isSelected={isSelected}
          onSelectedChange={onSelectedChange}
        />

        {
          columns.map((column) => {
            const {
              name,
              isVisible
            } = column;

            if (!isVisible) {
              return null;
            }

            if (name === 'tmdbId') {
              return (
                <TableRowCell key={name}>
                  {tmdbId}
                </TableRowCell>
              );
            }

            if (name === 'movieTitle') {
              return (
                <TableRowCell key={name}>
                  {movieTitle}
                </TableRowCell>
              );
            }

            if (name === 'movieYear') {
              return (
                <TableRowCell key={name}>
                  {movieYear}
                </TableRowCell>
              );
            }

            if (name === 'actions') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.actions}
                >
                  <IconButton
                    title={translate('RemoveFromBlocklist')}
                    name={icons.EDIT}
                    onPress={this.onEditImportExclusionPress}
                  />

                  <IconButton
                    title={translate('RemoveFromBlocklist')}
                    name={icons.REMOVE}
                    kind={kinds.DANGER}
                    onPress={this.onDeleteImportExclusionPress}
                  />
                </TableRowCell>
              );
            }

            return null;
          })
        }

        <EditImportListExclusionModalConnector
          id={id}
          isOpen={this.state.isEditImportExclusionModalOpen}
          onModalClose={this.onEditImportExclusionModalClose}
          onDeleteImportExclusionPress={this.onDeleteImportExclusionPress}
        />

        <ConfirmModal
          isOpen={this.state.isDeleteImportExclusionModalOpen}
          kind={kinds.DANGER}
          title={translate('DeleteImportListExclusion')}
          message={translate('AreYouSureYouWantToDeleteThisImportListExclusion')}
          confirmLabel={translate('Delete')}
          onConfirm={this.onConfirmDeleteImportExclusion}
          onCancel={this.onDeleteImportExclusionModalClose}
        />
      </TableRow>
    );
  }
}

ImportListExclusion.propTypes = {
  id: PropTypes.number.isRequired,
  movieTitle: PropTypes.string.isRequired,
  tmdbId: PropTypes.number.isRequired,
  movieYear: PropTypes.number.isRequired,
  isSelected: PropTypes.bool.isRequired,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  onSelectedChange: PropTypes.func.isRequired,
  onConfirmDeleteImportExclusion: PropTypes.func.isRequired
};

ImportListExclusion.defaultProps = {
  // The drag preview will not connect the drag handle.
  connectDragSource: (node) => node
};

export default ImportListExclusion;
