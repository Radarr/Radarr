import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { icons } from 'Helpers/Props';
import IconButton from 'Components/Link/IconButton';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import ExtraFileRow from './ExtraFileRow';
import styles from './ExtraFileTableContent.css';

const columns = [
  {
    name: 'relativePath',
    label: 'Extra File',
    isVisible: true
  },
  {
    name: 'extension',
    label: 'Extension',
    isVisible: true
  },
  {
    name: 'type',
    label: 'Type',
    isVisible: true
  },
  {
    name: 'action',
    label: React.createElement(IconButton, { name: icons.ADVANCED_SETTINGS }),
    isVisible: true
  }
];

class ExtraFileTableContent extends Component {

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
              No extra files to manage.
            </div>
        }

        {
          !!items.length &&
            <Table columns={columns}>
              <TableBody>
                {
                  items.map((item) => {
                    return (
                      <ExtraFileRow
                        key={item.id}
                        {...item}
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

ExtraFileTableContent.propTypes = {
  movieId: PropTypes.number,
  items: PropTypes.arrayOf(PropTypes.object).isRequired
};

export default ExtraFileTableContent;
