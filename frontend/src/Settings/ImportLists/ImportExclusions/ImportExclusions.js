import PropTypes from 'prop-types';
import React, { Component } from 'react';
import FieldSet from 'Components/FieldSet';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import PageSectionContent from 'Components/Page/PageSectionContent';
import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import EditImportExclusionModalConnector from './EditImportExclusionModalConnector';
import ImportExclusion from './ImportExclusion';
import styles from './ImportExclusions.css';

class ImportExclusions extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isAddImportExclusionModalOpen: false
    };
  }

  //
  // Listeners

  onAddImportExclusionPress = () => {
    this.setState({ isAddImportExclusionModalOpen: true });
  }

  onModalClose = () => {
    this.setState({ isAddImportExclusionModalOpen: false });
  }

  //
  // Render

  render() {
    const {
      items,
      onConfirmDeleteImportExclusion,
      ...otherProps
    } = this.props;

    return (
      <FieldSet legend={translate('ListExclusions')}>
        <PageSectionContent
          errorMessage={translate('UnableToLoadListExclusions')}
          {...otherProps}
        >
          <div className={styles.importExclusionsHeader}>
            <div className={styles.tmdbId}>TMDB Id</div>
            <div className={styles.title}>Title</div>
            <div className={styles.movieYear}>Year</div>
          </div>

          <div>
            {
              items.map((item, index) => {
                return (
                  <ImportExclusion
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

          <div className={styles.addImportExclusion}>
            <Link
              className={styles.addButton}
              onPress={this.onAddImportExclusionPress}
            >
              <Icon name={icons.ADD} />
            </Link>
          </div>

          <EditImportExclusionModalConnector
            isOpen={this.state.isAddImportExclusionModalOpen}
            onModalClose={this.onModalClose}
          />

        </PageSectionContent>
      </FieldSet>
    );
  }
}

ImportExclusions.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  onConfirmDeleteImportExclusion: PropTypes.func.isRequired
};

export default ImportExclusions;
