import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { selectImportListSchema, setImportListFieldValue, setImportListValue } from 'Store/Actions/settingsActions';
import createMovieCreditListSelector from 'Store/Selectors/createMovieCreditListSelector';

function createMapStateToProps() {
  return createMovieCreditListSelector();
}

const mapDispatchToProps = {
  selectImportListSchema,
  setImportListFieldValue,
  setImportListValue
};

class MovieCreditPosterConnector extends Component {

  //
  // Listeners

  onImportListSelect = () => {
    this.props.selectImportListSchema({ implementation: 'TMDbPersonImport', presetName: undefined });
    this.props.setImportListFieldValue({ name: 'personId', value: this.props.tmdbId.toString() });
    this.props.setImportListValue({ name: 'name', value: `${this.props.personName} - ${this.props.tmdbId}` });
  };

  //
  // Render

  render() {
    const {
      tmdbId,
      component: ItemComponent,
      personName
    } = this.props;

    return (
      <ItemComponent
        {...this.props}
        tmdbId={tmdbId}
        personName={personName}
        onImportListSelect={this.onImportListSelect}
      />
    );
  }
}

MovieCreditPosterConnector.propTypes = {
  tmdbId: PropTypes.number.isRequired,
  personName: PropTypes.string.isRequired,
  component: PropTypes.elementType.isRequired,
  selectImportListSchema: PropTypes.func.isRequired,
  setImportListFieldValue: PropTypes.func.isRequired,
  setImportListValue: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(MovieCreditPosterConnector);
