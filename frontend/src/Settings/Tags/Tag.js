import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Card from 'Components/Card';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import { kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import TagDetailsModal from './Details/TagDetailsModal';
import styles from './Tag.css';

class Tag extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isDetailsModalOpen: false,
      isDeleteTagModalOpen: false
    };
  }

  //
  // Listeners

  onShowDetailsPress = () => {
    this.setState({ isDetailsModalOpen: true });
  }

  onDetailsModalClose = () => {
    this.setState({ isDetailsModalOpen: false });
  }

  onDeleteTagPress = () => {
    this.setState({
      isDetailsModalOpen: false,
      isDeleteTagModalOpen: true
    });
  }

  onDeleteTagModalClose= () => {
    this.setState({ isDeleteTagModalOpen: false });
  }

  onConfirmDeleteTag = () => {
    this.props.onConfirmDeleteTag({ id: this.props.id });
  }

  //
  // Render

  render() {
    const {
      label,
      delayProfileIds,
      notificationIds,
      restrictionIds,
      netImportIds,
      movieIds
    } = this.props;

    const {
      isDetailsModalOpen,
      isDeleteTagModalOpen
    } = this.state;

    const isTagUsed = !!(
      delayProfileIds.length ||
      notificationIds.length ||
      restrictionIds.length ||
      netImportIds.length ||
      movieIds.length
    );

    return (
      <Card
        className={styles.tag}
        overlayContent={true}
        onPress={this.onShowDetailsPress}
      >
        <div className={styles.label}>
          {label}
        </div>

        {
          isTagUsed &&
            <div>
              {
                !!movieIds.length &&
                  <div>
                    {movieIds.length} movies
                  </div>
              }

              {
                !!delayProfileIds.length &&
                  <div>
                    {delayProfileIds.length} delay profile{delayProfileIds.length > 1 && 's'}
                  </div>
              }

              {
                !!notificationIds.length &&
                  <div>
                    {notificationIds.length} connection{notificationIds.length > 1 && 's'}
                  </div>
              }

              {
                !!restrictionIds.length &&
                  <div>
                    {restrictionIds.length} restriction{restrictionIds.length > 1 && 's'}
                  </div>
              }

              {
                !!netImportIds.length &&
                  <div>
                    {netImportIds.length} list{netImportIds.length > 1 && 's'}
                  </div>
              }
            </div>
        }

        {
          !isTagUsed &&
            <div>
              No links
            </div>
        }

        <TagDetailsModal
          label={label}
          isTagUsed={isTagUsed}
          movieIds={movieIds}
          delayProfileIds={delayProfileIds}
          notificationIds={notificationIds}
          restrictionIds={restrictionIds}
          netImportIds={netImportIds}
          isOpen={isDetailsModalOpen}
          onModalClose={this.onDetailsModalClose}
          onDeleteTagPress={this.onDeleteTagPress}
        />

        <ConfirmModal
          isOpen={isDeleteTagModalOpen}
          kind={kinds.DANGER}
          title="Delete Tag"
          message={`Are you sure you want to delete the tag '${label}'?`}
          confirmLabel={translate('Delete')}
          onConfirm={this.onConfirmDeleteTag}
          onCancel={this.onDeleteTagModalClose}
        />
      </Card>
    );
  }
}

Tag.propTypes = {
  id: PropTypes.number.isRequired,
  label: PropTypes.string.isRequired,
  delayProfileIds: PropTypes.arrayOf(PropTypes.number).isRequired,
  notificationIds: PropTypes.arrayOf(PropTypes.number).isRequired,
  restrictionIds: PropTypes.arrayOf(PropTypes.number).isRequired,
  netImportIds: PropTypes.arrayOf(PropTypes.number).isRequired,
  movieIds: PropTypes.arrayOf(PropTypes.number).isRequired,
  onConfirmDeleteTag: PropTypes.func.isRequired
};

Tag.defaultProps = {
  delayProfileIds: [],
  notificationIds: [],
  restrictionIds: [],
  netImportIds: [],
  movieIds: []
};

export default Tag;
