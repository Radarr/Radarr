import PropTypes from 'prop-types';
import React, { Component } from 'react';
import FieldSet from 'Components/FieldSet';
import PageSectionContent from 'Components/Page/PageSectionContent';
import translate from 'Utilities/String/translate';
import QualityDefinitionConnector from './QualityDefinitionConnector';
import styles from './QualityDefinitions.css';

class QualityDefinitions extends Component {

  //
  // Render

  render() {
    const {
      items,
      advancedSettings,
      ...otherProps
    } = this.props;

    return (
      <FieldSet legend={translate('QualityDefinitions')}>
        <PageSectionContent
          errorMessage="Unable to load Quality Definitions"
          {...otherProps}
        >
          <div className={styles.header}>
            <div className={styles.quality}>Quality</div>
            <div className={styles.title}>Title</div>
            <div className={styles.sizeLimit}>Size Limit</div>

            {
              advancedSettings ?
                <div className={styles.megabytesPerMinute}>
                  Megabytes Per Minute
                </div> :
                null
            }
          </div>

          <div className={styles.definitions}>
            {
              items.map((item) => {
                return (
                  <QualityDefinitionConnector
                    key={item.id}
                    {...item}
                    advancedSettings={advancedSettings}
                  />
                );
              })
            }
          </div>

          <div className={styles.sizeLimitHelpTextContainer}>
            <div className={styles.sizeLimitHelpText}>
              Limits are automatically adjusted for the movie runtime.
            </div>
          </div>
        </PageSectionContent>
      </FieldSet>
    );
  }
}

QualityDefinitions.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  defaultProfile: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  advancedSettings: PropTypes.bool.isRequired
};

export default QualityDefinitions;
