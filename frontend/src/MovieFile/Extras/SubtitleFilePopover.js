import PropTypes from 'prop-types';
import React, { Component } from 'react';
import IconButton from 'Components/Link/IconButton';
import Popover from 'Components/Tooltip/Popover';
import { icons, tooltipPositions } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './SubtitleFilePopover.css';

class SubtitleFilePopover extends Component {

  //
  // Render

  render() {
    const {
      title,
      languageTags
    } = this.props;

    return (
      <Popover
        anchor={
          <IconButton
            name={icons.INFO}
          />
        }
        title={translate('Tags')}
        body={
          <table>
            {
              title == null ?
                null :
                <tr>
                  <td className={styles.rowlabel}>{translate('Title')}</td>
                  <td>{title}</td>
                </tr>
            }
            {
              languageTags.map((tag, idx) => {
                return (<tr key={idx}>
                  <td className={styles.rowlabel}>{idx === 0 ? translate('Disposition') : ''}</td>
                  <td>{tag}</td>
                </tr>)
                ;
              })
            }
          </table>
        }
        position={tooltipPositions.LEFT}
      />
    );
  }
}

SubtitleFilePopover.propTypes = {
  title: PropTypes.string,
  languageTags: PropTypes.array
};

export default SubtitleFilePopover;
