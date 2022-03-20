import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Card from 'Components/Card';
import FieldSet from 'Components/FieldSet';
import Icon from 'Components/Icon';
import PageSectionContent from 'Components/Page/PageSectionContent';
import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import EditRestrictionModalConnector from './EditRestrictionModalConnector';
import Restriction from './Restriction';
import styles from './Restrictions.css';

class Restrictions extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isAddRestrictionModalOpen: false
    };
  }

  //
  // Listeners

  onAddRestrictionPress = () => {
    this.setState({ isAddRestrictionModalOpen: true });
  };

  onAddRestrictionModalClose = () => {
    this.setState({ isAddRestrictionModalOpen: false });
  };

  //
  // Render

  render() {
    const {
      items,
      tagList,
      onConfirmDeleteRestriction,
      ...otherProps
    } = this.props;

    return (
      <FieldSet legend={translate('Restrictions')}>
        <PageSectionContent
          errorMessage={translate('UnableToLoadRestrictions')}
          {...otherProps}
        >
          <div className={styles.restrictions}>
            <Card
              className={styles.addRestriction}
              onPress={this.onAddRestrictionPress}
            >
              <div className={styles.center}>
                <Icon
                  name={icons.ADD}
                  size={45}
                />
              </div>
            </Card>

            {
              items.map((item) => {
                return (
                  <Restriction
                    key={item.id}
                    tagList={tagList}
                    {...item}
                    onConfirmDeleteRestriction={onConfirmDeleteRestriction}
                  />
                );
              })
            }
          </div>

          <EditRestrictionModalConnector
            isOpen={this.state.isAddRestrictionModalOpen}
            onModalClose={this.onAddRestrictionModalClose}
          />
        </PageSectionContent>
      </FieldSet>
    );
  }
}

Restrictions.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  tagList: PropTypes.arrayOf(PropTypes.object).isRequired,
  onConfirmDeleteRestriction: PropTypes.func.isRequired
};

export default Restrictions;
