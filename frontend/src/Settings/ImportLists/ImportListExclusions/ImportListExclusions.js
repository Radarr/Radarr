import PropTypes from 'prop-types';
import React, { Component } from 'react';
import FieldSet from 'Components/FieldSet';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import PageSectionContent from 'Components/Page/PageSectionContent';
import { icons, kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import EditImportListExclusionModalConnector from './EditImportListExclusionModalConnector';
import ImportListExclusion from './ImportListExclusion';
import styles from './ImportListExclusions.css';

class ImportListExclusions extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isAddImportExclusionModalOpen: false,
      isPurgeImportExclusionModalOpen: false
    };
  }

  //
  // Listeners

  onAddImportExclusionPress = () => {
    this.setState({ isAddImportExclusionModalOpen: true });
  };

  onModalClose = () => {
    this.setState({ isAddImportExclusionModalOpen: false });
  };

  onPurgeImportExclusionPress = () => {
    this.setState({ isPurgeImportExclusionModalOpen: true });
  };

  onPurgeImportExclusionModalClose = () => {
    this.setState({ isPurgeImportExclusionModalOpen: false });
  };

  onConfirmPurgeImportExclusions = () => {
    this.props.onConfirmPurgeImportExclusions();
    this.onPurgeImportExclusionModalClose();
  };

  //
  // Render

  render() {
    const {
      items,
      onConfirmDeleteImportExclusion,
      ...otherProps
    } = this.props;

    return (
      <FieldSet legend={translate('ImportListExclusions')}>
        <PageSectionContent
          errorMessage={translate('ImportListExclusionsLoadError')}
          {...otherProps}
        >
          <div className={styles.importListExclusionsHeader}>
            <div className={styles.tmdbId}>
              {translate('TMDBId')}
            </div>
            <div className={styles.title}>
              {translate('Title')}
            </div>
            <div className={styles.movieYear}>
              {translate('Year')}
            </div>
          </div>

          <div>
            {
              items.map((item, index) => {
                return (
                  <ImportListExclusion
                    key={item.id}
                    {...item}
                    {...otherProps}
                    index={index}
                    onConfirmDeleteImportExclusion={onConfirmDeleteImportExclusion}
                  />
                );
              })
            }
          </div>

          <div className={styles.footerButtons}>
            <Link
              className={styles.purgeButton}
              onPress={this.onPurgeImportExclusionPress}
            >
              <Icon name={icons.DELETE} />
            </Link>

            <Link
              className={styles.addButton}
              onPress={this.onAddImportExclusionPress}
            >
              <Icon name={icons.ADD} />
            </Link>
          </div>

          <EditImportListExclusionModalConnector
            isOpen={this.state.isAddImportExclusionModalOpen}
            onModalClose={this.onModalClose}
          />

          <ConfirmModal
            isOpen={this.state.isPurgeImportExclusionModalOpen}
            kind={kinds.DANGER}
            title={translate('PurgeImportListExclusions')}
            message={translate('PurgeImportListExclusionMessageText')}
            confirmLabel={translate('Purge')}
            onConfirm={this.onConfirmPurgeImportExclusions}
            onCancel={this.onPurgeImportExclusionModalClose}
          />
        </PageSectionContent>
      </FieldSet>
    );
  }
}

ImportListExclusions.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  onConfirmDeleteImportExclusion: PropTypes.func.isRequired,
  onConfirmPurgeImportExclusions: PropTypes.func.isRequired
};

export default ImportListExclusions;
