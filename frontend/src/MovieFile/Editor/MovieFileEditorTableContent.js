import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { icons } from 'Helpers/Props';
import IconButton from 'Components/Link/IconButton';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import MovieFileEditorRow from './MovieFileEditorRow';
import styles from './MovieFileEditorTableContent.css';

const columns = [
  {
    name: 'title',
    label: 'Title',
    isVisible: true
  },
  {
    name: 'videoCodec',
    label: 'Video Codec',
    isVisible: true
  },
  {
    name: 'audioInfo',
    label: 'Audio Info',
    isVisible: true
  },
  {
    name: 'size',
    label: 'Size',
    isVisible: true
  },
  {
    name: 'languages',
    label: 'Languages',
    isVisible: true
  },
  {
    name: 'quality',
    label: 'Quality',
    isVisible: true
  },
  {
    name: 'quality.customFormats',
    label: 'Formats',
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
