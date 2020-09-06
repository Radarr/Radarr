import PropTypes from 'prop-types';
import React from 'react';
import FieldSet from 'Components/FieldSet';
import PageSectionContent from 'Components/Page/PageSectionContent';
import translate from 'Utilities/String/translate';
import TagConnector from './TagConnector';
import styles from './Tags.css';

function Tags(props) {
  const {
    items,
    ...otherProps
  } = props;

  if (!items.length) {
    return (
      <div>
        {translate('NoTagsHaveBeenAddedYet')}
      </div>
    );
  }

  return (
    <FieldSet
      legend={translate('Tags')}
    >
      <PageSectionContent
        errorMessage={translate('UnableToLoadTags')}
        {...otherProps}
      >
        <div className={styles.tags}>
          {
            items.map((item) => {
              return (
                <TagConnector
                  key={item.id}
                  {...item}
                />
              );
            })
          }
        </div>
      </PageSectionContent>
    </FieldSet>
  );
}

Tags.propTypes = {
  items: PropTypes.arrayOf(PropTypes.object).isRequired
};

export default Tags;
