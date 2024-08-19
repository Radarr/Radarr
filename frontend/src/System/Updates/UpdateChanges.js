import PropTypes from 'prop-types';
import React, { Component } from 'react';
import InlineMarkdown from 'Components/Markdown/InlineMarkdown';
import styles from './UpdateChanges.css';

class UpdateChanges extends Component {

  //
  // Render

  render() {
    const {
      title,
      changes
    } = this.props;

    if (changes.length === 0) {
      return null;
    }

    const uniqueChanges = [...new Set(changes)];

    return (
      <div>
        <div className={styles.title}>{title}</div>
        <ul>
          {
            uniqueChanges.map((change, index) => {
              const checkChange = change.replace(/#\d{4,5}\b/g, (match, contents) => {
                return `[${match}](https://github.com/Radarr/Radarr/issues/${match.substring(1)})`;
              });

              return (
                <li key={index}>
                  <InlineMarkdown data={checkChange} />
                </li>
              );
            })
          }
        </ul>
      </div>
    );
  }

}

UpdateChanges.propTypes = {
  title: PropTypes.string.isRequired,
  changes: PropTypes.arrayOf(PropTypes.string)
};

export default UpdateChanges;
