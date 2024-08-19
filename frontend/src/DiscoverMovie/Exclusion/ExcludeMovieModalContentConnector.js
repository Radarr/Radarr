import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { addImportListExclusions } from 'Store/Actions/discoverMovieActions';
import ExcludeMovieModalContent from './ExcludeMovieModalContent';

const mapDispatchToProps = {
  addImportListExclusions
};

class ExcludeMovieModalContentConnector extends Component {

  //
  // Listeners

  onExcludePress = () => {
    this.props.addImportListExclusions({ ids: [this.props.tmdbId] });

    this.props.onModalClose(true);
  };

  //
  // Render

  render() {
    return (
      <ExcludeMovieModalContent
        {...this.props}
        onExcludePress={this.onExcludePress}
      />
    );
  }
}

ExcludeMovieModalContentConnector.propTypes = {
  tmdbId: PropTypes.number.isRequired,
  title: PropTypes.string.isRequired,
  year: PropTypes.number.isRequired,
  onModalClose: PropTypes.func.isRequired,
  addImportListExclusions: PropTypes.func.isRequired
};

export default connect(undefined, mapDispatchToProps)(ExcludeMovieModalContentConnector);
