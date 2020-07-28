import classNames from 'classnames';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import { icons, kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import EditNetImportExclusionModalConnector from './EditNetImportExclusionModalConnector';
import styles from './NetImportExclusion.css';

class NetImportExclusion extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isEditNetImportExclusionModalOpen: false,
      isDeleteNetImportExclusionModalOpen: false
    };
  }

  //
  // Listeners

  onEditNetImportExclusionPress = () => {
    this.setState({ isEditNetImportExclusionModalOpen: true });
  }

  onEditNetImportExclusionModalClose = () => {
    this.setState({ isEditNetImportExclusionModalOpen: false });
  }

  onDeleteNetImportExclusionPress = () => {
    this.setState({
      isEditNetImportExclusionModalOpen: false,
      isDeleteNetImportExclusionModalOpen: true
    });
  }

  onDeleteNetImportExclusionModalClose = () => {
    this.setState({ isDeleteNetImportExclusionModalOpen: false });
  }

  onConfirmDeleteNetImportExclusion = () => {
    this.props.onConfirmDeleteNetImportExclusion(this.props.id);
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
          styles.netImportExclusion
        )}
      >
        <div className={styles.tmdbId}>{tmdbId}</div>
        <div className={styles.movieTitle}>{movieTitle}</div>
        <div className={styles.movieYear}>{movieYear}</div>

        <div className={styles.actions}>
          <Link
            onPress={this.onEditNetImportExclusionPress}
          >
            <Icon name={icons.EDIT} />
          </Link>
        </div>

        <EditNetImportExclusionModalConnector
          id={id}
          isOpen={this.state.isEditNetImportExclusionModalOpen}
          onModalClose={this.onEditNetImportExclusionModalClose}
          onDeleteNetImportExclusionPress={this.onDeleteNetImportExclusionPress}
        />

        <ConfirmModal
          isOpen={this.state.isDeleteNetImportExclusionModalOpen}
          kind={kinds.DANGER}
          title="Delete Import List Exclusion"
          message="Are you sure you want to delete this import list exclusion?"
          confirmLabel={translate('Delete')}
          onConfirm={this.onConfirmDeleteNetImportExclusion}
          onCancel={this.onDeleteNetImportExclusionModalClose}
        />
      </div>
    );
  }
}

NetImportExclusion.propTypes = {
  id: PropTypes.number.isRequired,
  movieTitle: PropTypes.string.isRequired,
  tmdbId: PropTypes.number.isRequired,
  movieYear: PropTypes.number.isRequired,
  onConfirmDeleteNetImportExclusion: PropTypes.func.isRequired
};

NetImportExclusion.defaultProps = {
  // The drag preview will not connect the drag handle.
  connectDragSource: (node) => node
};

export default NetImportExclusion;
