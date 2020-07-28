import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { selectNetImportSchema, setNetImportFieldValue, setNetImportValue } from 'Store/Actions/settingsActions';
import createMovieCreditListSelector from 'Store/Selectors/createMovieCreditListSelector';

function createMapStateToProps() {
  return createMovieCreditListSelector();
}

const mapDispatchToProps = {
  selectNetImportSchema,
  setNetImportFieldValue,
  setNetImportValue
};

class MovieCreditPosterConnector extends Component {

  //
  // Listeners

  onNetImportSelect = () => {
    this.props.selectNetImportSchema({ implementation: 'TMDbPersonImport', presetName: undefined });
    this.props.setNetImportFieldValue({ name: 'personId', value: this.props.tmdbId.toString() });
    this.props.setNetImportValue({ name: 'name', value: `${this.props.personName} - ${this.props.tmdbId}` });
  }

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
        onNetImportSelect={this.onNetImportSelect}
      />
    );
  }
}

MovieCreditPosterConnector.propTypes = {
  tmdbId: PropTypes.number.isRequired,
  personName: PropTypes.string.isRequired,
  component: PropTypes.elementType.isRequired,
  selectNetImportSchema: PropTypes.func.isRequired,
  setNetImportFieldValue: PropTypes.func.isRequired,
  setNetImportValue: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(MovieCreditPosterConnector);
