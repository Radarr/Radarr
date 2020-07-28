import PropTypes from 'prop-types';
import React, { Component } from 'react';
import FieldSet from 'Components/FieldSet';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import PageSectionContent from 'Components/Page/PageSectionContent';
import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import EditNetImportExclusionModalConnector from './EditNetImportExclusionModalConnector';
import NetImportExclusion from './NetImportExclusion';
import styles from './NetImportExclusions.css';

class NetImportExclusions extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isAddNetImportExclusionModalOpen: false
    };
  }

  //
  // Listeners

  onAddNetImportExclusionPress = () => {
    this.setState({ isAddNetImportExclusionModalOpen: true });
  }

  onModalClose = () => {
    this.setState({ isAddNetImportExclusionModalOpen: false });
  }

  //
  // Render

  render() {
    const {
      items,
      onConfirmDeleteNetImportExclusion,
      ...otherProps
    } = this.props;

    return (
      <FieldSet legend={translate('ListExclusions')}>
        <PageSectionContent
          errorMessage="Unable to load List Exclusions"
          {...otherProps}
        >
          <div className={styles.netImportExclusionsHeader}>
            <div className={styles.tmdbId}>TMDB Id</div>
            <div className={styles.title}>Title</div>
            <div className={styles.movieYear}>Year</div>
          </div>

          <div>
            {
              items.map((item, index) => {
                return (
                  <NetImportExclusion
                    key={item.id}
                    {...item}
                    {...otherProps}
                    index={index}
                    onConfirmDeleteNetImportExclusion={onConfirmDeleteNetImportExclusion}
                  />
                );
              })
            }
          </div>

          <div className={styles.addNetImportExclusion}>
            <Link
              className={styles.addButton}
              onPress={this.onAddNetImportExclusionPress}
            >
              <Icon name={icons.ADD} />
            </Link>
          </div>

          <EditNetImportExclusionModalConnector
            isOpen={this.state.isAddNetImportExclusionModalOpen}
            onModalClose={this.onModalClose}
          />

        </PageSectionContent>
      </FieldSet>
    );
  }
}

NetImportExclusions.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  onConfirmDeleteNetImportExclusion: PropTypes.func.isRequired
};

export default NetImportExclusions;
