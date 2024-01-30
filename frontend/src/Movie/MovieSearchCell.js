import PropTypes from 'prop-types';
import React, { Component } from 'react';
import IconButton from 'Components/Link/IconButton';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import MovieInteractiveSearchModalConnector from './Search/MovieInteractiveSearchModalConnector';
import styles from './MovieSearchCell.css';

class MovieSearchCell extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isInteractiveSearchModalOpen: false
    };
  }

  //
  // Listeners

  onManualSearchPress = () => {
    this.setState({ isInteractiveSearchModalOpen: true });
  };

  onInteractiveSearchModalClose = () => {
    this.setState({ isInteractiveSearchModalOpen: false });
  };

  //
  // Render

  render() {
    const {
      movieId,
      movieTitle,
      isSearching,
      onSearchPress,
      ...otherProps
    } = this.props;

    return (
      <TableRowCell className={styles.movieSearchCell}>
        <SpinnerIconButton
          name={icons.SEARCH}
          isSpinning={isSearching}
          onPress={onSearchPress}
          title={translate('AutomaticSearch')}
        />

        <IconButton
          name={icons.INTERACTIVE}
          onPress={this.onManualSearchPress}
          title={translate('InteractiveSearch')}
        />

        <MovieInteractiveSearchModalConnector
          isOpen={this.state.isInteractiveSearchModalOpen}
          movieId={movieId}
          movieTitle={movieTitle}
          onModalClose={this.onInteractiveSearchModalClose}
          {...otherProps}
        />
      </TableRowCell>
    );
  }
}

MovieSearchCell.propTypes = {
  movieId: PropTypes.number.isRequired,
  movieTitle: PropTypes.string.isRequired,
  isSearching: PropTypes.bool.isRequired,
  onSearchPress: PropTypes.func.isRequired
};

export default MovieSearchCell;
