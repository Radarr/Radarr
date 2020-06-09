import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { addNetImportExclusions } from 'Store/Actions/discoverMovieActions';
import ExcludeMovieModalContent from './ExcludeMovieModalContent';

const mapDispatchToProps = {
  addNetImportExclusions
};

class ExcludeMovieModalContentConnector extends Component {

  //
  // Listeners

  onExcludePress = () => {
    this.props.addNetImportExclusions([{
      tmdbId: this.props.tmdbId,
      movieTitle: this.props.title,
      movieYear: this.props.year
    }]);

    this.props.onModalClose(true);
  }

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
  addNetImportExclusions: PropTypes.func.isRequired
};

export default connect(undefined, mapDispatchToProps)(ExcludeMovieModalContentConnector);
