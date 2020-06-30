import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { inputTypes } from 'Helpers/Props';
import TableRow from 'Components/Table/TableRow';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import FormInputGroup from 'Components/Form/FormInputGroup';
import titleCase from 'Utilities/String/titleCase';

class SelectEditionRow extends Component {

  //
  // Listeners

  onInputChange = ({ name, value }) => {
    this.props.onEditionSelect(parseInt(name), parseInt(value));
  }

  //
  // Render

  render() {
    const {
      id,
      matchedEditionId,
      title,
      disambiguation,
      editions,
      columns
    } = this.props;

    const extendedTitle = disambiguation ? `${title} (${disambiguation})` : title;

    const values = _.map(editions, (bookEdition) => {

      let value = `${bookEdition.title}`;

      if (bookEdition.disambiguation) {
        value = `${value} (${titleCase(bookEdition.disambiguation)})`;
      }

      const extras = [];
      if (bookEdition.language) {
        extras.push(bookEdition.language);
      }
      if (bookEdition.publisher) {
        extras.push(bookEdition.publisher);
      }
      if (bookEdition.isbn13) {
        extras.push(bookEdition.isbn13);
      }
      if (bookEdition.format) {
        extras.push(bookEdition.format);
      }
      if (bookEdition.pageCount > 0) {
        extras.push(`${bookEdition.pageCount}p`);
      }

      if (extras) {
        value = `${value} [${extras.join(', ')}]`;
      }

      return {
        key: bookEdition.id,
        value
      };
    });

    const sortedValues = _.orderBy(values, ['value']);

    return (
      <TableRow>
        {
          columns.map((column) => {
            const {
              name,
              isVisible
            } = column;

            if (!isVisible) {
              return null;
            }

            if (name === 'book') {
              return (
                <TableRowCell key={name}>
                  {extendedTitle}
                </TableRowCell>
              );
            }

            if (name === 'edition') {
              return (
                <TableRowCell key={name}>
                  <FormInputGroup
                    type={inputTypes.SELECT}
                    name={id.toString()}
                    values={sortedValues}
                    value={matchedEditionId}
                    onChange={this.onInputChange}
                  />
                </TableRowCell>
              );
            }

            return null;
          })
        }
      </TableRow>

    );
  }
}

SelectEditionRow.propTypes = {
  id: PropTypes.number.isRequired,
  matchedEditionId: PropTypes.number.isRequired,
  title: PropTypes.string.isRequired,
  disambiguation: PropTypes.string.isRequired,
  editions: PropTypes.arrayOf(PropTypes.object).isRequired,
  onEditionSelect: PropTypes.func.isRequired,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired
};

export default SelectEditionRow;
