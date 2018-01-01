import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Link from 'Components/Link/Link';
import AlbumDetailsModal from 'Album/AlbumDetailsModal';
import styles from './AlbumTitleLink.css';

class AlbumTitleLink extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isDetailsModalOpen: false
    };
  }

  //
  // Listeners

  onLinkPress = () => {
    this.setState({ isDetailsModalOpen: true });
  }

  onModalClose = () => {
    this.setState({ isDetailsModalOpen: false });
  }

  //
  // Render

  render() {
    const {
      albumTitle,
      ...otherProps
    } = this.props;

    return (
      <div>
        <Link
          className={styles.link}
          onPress={this.onLinkPress}
        >
          {albumTitle}
        </Link>

        <AlbumDetailsModal
          isOpen={this.state.isDetailsModalOpen}
          albumTitle={albumTitle}
          {...otherProps}
          onModalClose={this.onModalClose}
        />
      </div>
    );
  }
}

AlbumTitleLink.propTypes = {
  albumTitle: PropTypes.string.isRequired
};

AlbumTitleLink.defaultProps = {
  showArtistButton: false
};

export default AlbumTitleLink;
