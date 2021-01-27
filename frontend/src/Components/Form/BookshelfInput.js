import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableSelectCell from 'Components/Table/Cells/TableSelectCell';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import TableRow from 'Components/Table/TableRow';
import tagShape from 'Helpers/Props/Shapes/tagShape';
import getSelectedIds from 'Utilities/Table/getSelectedIds';
import selectAll from 'Utilities/Table/selectAll';
import toggleSelected from 'Utilities/Table/toggleSelected';
import styles from './BookshelfInput.css';

const columns = [
  {
    name: 'name',
    label: 'Bookshelf',
    isSortable: false,
    isVisible: true
  }
];

class BookshelfInput extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    const initialSelection = _.mapValues(_.keyBy(props.value), () => true);

    this.state = {
      allSelected: false,
      allUnselected: false,
      selectedState: initialSelection
    };
  }

  componentDidUpdate(prevProps, prevState) {
    const {
      name,
      onChange
    } = this.props;

    const oldSelected = getSelectedIds(prevState.selectedState, { parseIds: false }).sort();
    const newSelected = this.getSelectedIds().sort();

    if (!_.isEqual(oldSelected, newSelected)) {
      onChange({
        name,
        value: newSelected
      });
    }
  }

  //
  // Control

  getSelectedIds = () => {
    return getSelectedIds(this.state.selectedState, { parseIds: false });
  }

  //
  // Listeners

  onSelectAllChange = ({ value }) => {
    this.setState(selectAll(this.state.selectedState, value));
  }

  onSelectedChange = ({ id, value, shiftKey = false }) => {
    this.setState((state, props) => {
      return toggleSelected(state, props.items, id, value, shiftKey);
    });
  }

  //
  // Render

  render() {
    const {
      className,
      helptext,
      items,
      user,
      isFetching,
      isPopulated
    } = this.props;

    const {
      allSelected,
      allUnselected,
      selectedState
    } = this.state;

    return (
      <div className={className}>
        {
          isFetching &&
            <LoadingIndicator />
        }

        {
          !isPopulated && !isFetching &&
            <div>
              Authenticate with Goodreads to retrieve bookshelves.
            </div>
        }

        {
          isPopulated && !isFetching && !user &&
            <div>
              Could not retrieve data from Goodreads.  Try re-authenticating.
            </div>
        }

        {
          isPopulated && !isFetching && user && !items.length &&
            <div>
              No bookshelves found for Goodreads user {user}.
            </div>
        }

        {
          isPopulated && !isFetching && user && !!items.length &&
            <div className={className}>
              {helptext}
              <Table
                columns={columns}
                selectAll={true}
                allSelected={allSelected}
                allUnselected={allUnselected}
                onSelectAllChange={this.onSelectAllChange}
              >
                <TableBody>
                  {
                    items.map((item) => {
                      return (
                        <TableRow
                          key={item.id}
                        >
                          <TableSelectCell
                            id={item.id}
                            isSelected={selectedState[item.id]}
                            onSelectedChange={this.onSelectedChange}
                          />

                          <TableRowCell
                            className={styles.relativePath}
                            title={item.name}
                          >
                            {item.name}
                          </TableRowCell>
                        </TableRow>
                      );
                    })
                  }
                </TableBody>
              </Table>
            </div>
        }
      </div>
    );
  }
}

BookshelfInput.propTypes = {
  className: PropTypes.string.isRequired,
  name: PropTypes.string.isRequired,
  value: PropTypes.arrayOf(PropTypes.oneOfType([PropTypes.number, PropTypes.string])).isRequired,
  helptext: PropTypes.string.isRequired,
  user: PropTypes.string.isRequired,
  items: PropTypes.arrayOf(PropTypes.shape(tagShape)).isRequired,
  hasError: PropTypes.bool,
  hasWarning: PropTypes.bool,
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  onChange: PropTypes.func.isRequired
};

BookshelfInput.defaultProps = {
  className: styles.bookshelfInputWrapper,
  inputClassName: styles.input,
  isPopulated: false
};

export default BookshelfInput;
