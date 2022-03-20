import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Card from 'Components/Card';
import Label from 'Components/Label';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import TagList from 'Components/TagList';
import { kinds } from 'Helpers/Props';
import split from 'Utilities/String/split';
import translate from 'Utilities/String/translate';
import EditRestrictionModalConnector from './EditRestrictionModalConnector';
import styles from './Restriction.css';

class Restriction extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isEditRestrictionModalOpen: false,
      isDeleteRestrictionModalOpen: false
    };
  }

  //
  // Listeners

  onEditRestrictionPress = () => {
    this.setState({ isEditRestrictionModalOpen: true });
  };

  onEditRestrictionModalClose = () => {
    this.setState({ isEditRestrictionModalOpen: false });
  };

  onDeleteRestrictionPress = () => {
    this.setState({
      isEditRestrictionModalOpen: false,
      isDeleteRestrictionModalOpen: true
    });
  };

  onDeleteRestrictionModalClose= () => {
    this.setState({ isDeleteRestrictionModalOpen: false });
  };

  onConfirmDeleteRestriction = () => {
    this.props.onConfirmDeleteRestriction(this.props.id);
  };

  //
  // Render

  render() {
    const {
      id,
      required,
      ignored,
      tags,
      tagList
    } = this.props;

    return (
      <Card
        className={styles.restriction}
        overlayContent={true}
        onPress={this.onEditRestrictionPress}
      >
        <div>
          {
            split(required).map((item) => {
              if (!item) {
                return null;
              }

              return (
                <Label
                  key={item}
                  kind={kinds.SUCCESS}
                >
                  {item}
                </Label>
              );
            })
          }
        </div>

        <div>
          {
            split(ignored).map((item) => {
              if (!item) {
                return null;
              }

              return (
                <Label
                  key={item}
                  kind={kinds.DANGER}
                >
                  {item}
                </Label>
              );
            })
          }
        </div>

        <TagList
          tags={tags}
          tagList={tagList}
        />

        <EditRestrictionModalConnector
          id={id}
          isOpen={this.state.isEditRestrictionModalOpen}
          onModalClose={this.onEditRestrictionModalClose}
          onDeleteRestrictionPress={this.onDeleteRestrictionPress}
        />

        <ConfirmModal
          isOpen={this.state.isDeleteRestrictionModalOpen}
          kind={kinds.DANGER}
          title={translate('DeleteRestriction')}
          message={translate('DeleteRestrictionHelpText')}
          confirmLabel={translate('Delete')}
          onConfirm={this.onConfirmDeleteRestriction}
          onCancel={this.onDeleteRestrictionModalClose}
        />
      </Card>
    );
  }
}

Restriction.propTypes = {
  id: PropTypes.number.isRequired,
  required: PropTypes.string.isRequired,
  ignored: PropTypes.string.isRequired,
  tags: PropTypes.arrayOf(PropTypes.number).isRequired,
  tagList: PropTypes.arrayOf(PropTypes.object).isRequired,
  onConfirmDeleteRestriction: PropTypes.func.isRequired
};

Restriction.defaultProps = {
  required: '',
  ignored: ''
};

export default Restriction;
