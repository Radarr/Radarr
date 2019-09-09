import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { inputTypes } from 'Helpers/Props';
import TableRow from 'Components/Table/TableRow';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import FormInputGroup from 'Components/Form/FormInputGroup';
import titleCase from 'Utilities/String/titleCase';

class SelectAlbumReleaseRow extends Component {

  //
  // Listeners

  onInputChange = ({ name, value }) => {
    this.props.onAlbumReleaseSelect(parseInt(name), parseInt(value));
  }

  //
  // Render

  render() {
    const {
      id,
      matchedReleaseId,
      title,
      disambiguation,
      releases,
      columns
    } = this.props;

    const extendedTitle = disambiguation ? `${title} (${disambiguation})` : title;

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

            if (name === 'album') {
              return (
                <TableRowCell key={name}>
                  {extendedTitle}
                </TableRowCell>
              );
            }

            if (name === 'release') {
              return (
                <TableRowCell key={name}>
                  <FormInputGroup
                    type={inputTypes.SELECT}
                    name={id.toString()}
                    values={_.map(releases, (r) => ({
                      key: r.id,
                      value: `${r.title}` +
                        `${r.disambiguation ? ' (' : ''}${titleCase(r.disambiguation)}${r.disambiguation ? ')' : ''}` +
                        `, ${r.mediumCount} med, ${r.trackCount} tracks` +
                        `${r.country.length > 0 ? ', ' : ''}${r.country}` +
                        `${r.format ? ', [' : ''}${r.format}${r.format ? ']' : ''}` +
                        `${r.monitored ? ', Monitored' : ''}`
                    }))}
                    value={matchedReleaseId}
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

SelectAlbumReleaseRow.propTypes = {
  id: PropTypes.number.isRequired,
  matchedReleaseId: PropTypes.number.isRequired,
  title: PropTypes.string.isRequired,
  disambiguation: PropTypes.string.isRequired,
  releases: PropTypes.arrayOf(PropTypes.object).isRequired,
  onAlbumReleaseSelect: PropTypes.func.isRequired,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired
};

export default SelectAlbumReleaseRow;
