import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Icon from 'Components/Icon';
import IconButton from 'Components/Link/IconButton';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import MovieFileEditorRow from './MovieFileEditorRow';
import styles from './MovieFileEditorTableContent.css';

const columns = [
  {
    name: 'title',
    label: () => translate('RelativePath'),
    isVisible: true
  },
  {
    name: 'videoCodec',
    label: () => translate('VideoCodec'),
    isVisible: true
  },
  {
    name: 'audioInfo',
    label: () => translate('AudioInfo'),
    isVisible: true
  },
  {
    name: 'audioLanguages',
    label: () => translate('AudioLanguages'),
    isVisible: true
  },
  {
    name: 'subtitleLanguages',
    label: () => translate('SubtitleLanguages'),
    isVisible: true
  },
  {
    name: 'size',
    label: () => translate('Size'),
    isVisible: true
  },
  {
    name: 'languages',
    label: () => translate('Languages'),
    isVisible: true
  },
  {
    name: 'quality',
    label: () => translate('Quality'),
    isVisible: true
  },
  {
    name: 'releaseGroup',
    label: () => translate('ReleaseGroup'),
    isVisible: true
  },
  {
    name: 'customFormats',
    label: () => translate('Formats'),
    isVisible: true
  },
  {
    name: 'customFormatScore',
    label: React.createElement(Icon, {
      name: icons.SCORE,
      title: () => translate('CustomFormatScore')
    }),
    isVisible: true
  },
  {
    name: 'action',
    label: React.createElement(IconButton, { name: icons.ADVANCED_SETTINGS }),
    isVisible: true
  }
];

class MovieFileEditorTableContent extends Component {

  //
  // Render

  render() {
    const {
      items
    } = this.props;

    return (
      <div>
        {
          !items.length &&
            <div className={styles.blankpad}>
              No movie files to manage.
            </div>
        }

        {
          !!items.length &&
            <Table columns={columns}>
              <TableBody>
                {
                  items.map((item) => {
                    return (
                      <MovieFileEditorRow
                        key={item.id}
                        {...item}
                        onDeletePress={this.props.onDeletePress}
                      />
                    );
                  })
                }
              </TableBody>
            </Table>
        }

      </div>
    );
  }
}

MovieFileEditorTableContent.propTypes = {
  movieId: PropTypes.number,
  isDeleting: PropTypes.bool.isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  onDeletePress: PropTypes.func.isRequired
};

export default MovieFileEditorTableContent;
