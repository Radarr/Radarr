import classNames from 'classnames';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import { icons, kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import EditImportExclusionModalConnector from './EditImportExclusionModalConnector';
import styles from './ImportExclusion.css';

class ImportExclusion extends Component {

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
  }

  onEditImportExclusionModalClose = () => {
    this.setState({ isEditImportExclusionModalOpen: false });
  }

  onDeleteImportExclusionPress = () => {
    this.setState({
      isEditImportExclusionModalOpen: false,
      isDeleteImportExclusionModalOpen: true
    });
  }

  onDeleteImportExclusionModalClose = () => {
    this.setState({ isDeleteImportExclusionModalOpen: false });
  }

  onConfirmDeleteImportExclusion = () => {
    this.props.onConfirmDeleteImportExclusion(this.props.id);
  }

  //
  // Render

  render() {
    const {
      id,
      movieTitle,
      tmdbId,
      movieYear
    } = this.props;

    return (
      <div
        className={classNames(
          styles.importExclusion
        )}
      >
        <div className={styles.tmdbId}>{tmdbId}</div>
        <div className={styles.movieTitle}>{movieTitle}</div>
        <div className={styles.movieYear}>{movieYear}</div>

        <div className={styles.actions}>
          <Link
            onPress={this.onEditImportExclusionPress}
          >
            <Icon name={icons.EDIT} />
          </Link>
        </div>

        <EditImportExclusionModalConnector
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
      </div>
    );
  }
}

ImportExclusion.propTypes = {
  id: PropTypes.number.isRequired,
  movieTitle: PropTypes.string.isRequired,
  tmdbId: PropTypes.number.isRequired,
  movieYear: PropTypes.number.isRequired,
  onConfirmDeleteImportExclusion: PropTypes.func.isRequired
};

ImportExclusion.defaultProps = {
  // The drag preview will not connect the drag handle.
  connectDragSource: (node) => node
};

export default ImportExclusion;
