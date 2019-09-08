import PropTypes from 'prop-types';
import React, { Component } from 'react';
import HeartRating from 'Components/HeartRating';
import VirtualTableRow from 'Components/Table/VirtualTableRow';
import VirtualTableRowCell from 'Components/Table/Cells/VirtualTableRowCell';
import RelativeDateCellConnector from 'Components/Table/Cells/RelativeDateCellConnector';
import MovieStatusCell from './MovieStatusCell';
import Link from 'Components/Link/Link';
import AddNewMovieModal from 'AddMovie/AddNewMovie/AddNewMovieModal';
import styles from './AddListMovieRow.css';

class AddListMovieRow extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isNewAddMovieModalOpen: false
    };
  }

  //
  // Listeners

  onPress = () => {
    this.setState({ isNewAddMovieModalOpen: true });
  }

  onAddMovieModalClose = () => {
    this.setState({ isNewAddMovieModalOpen: false });
  }

  //
  // Render

  render() {
    const {
      style,
      status,
      tmdbId,
      title,
      titleSlug,
      studio,
      inCinemas,
      physicalRelease,
      year,
      overview,
      folder,
      images,
      genres,
      ratings,
      certification,
      columns,
      isExistingMovie
    } = this.props;

    const {
      isNewAddMovieModalOpen
    } = this.state;

    const linkProps = isExistingMovie ? { to: `/movie/${titleSlug}` } : { onPress: this.onPress };

    return (
      <div>
        <VirtualTableRow style={style}>
          {
            columns.map((column) => {
              const {
                name,
                isVisible
              } = column;

              if (!isVisible) {
                return null;
              }

              if (name === 'status') {
                return (
                  <MovieStatusCell
                    key={name}
                    className={styles[name]}
                    // monitored={monitored}
                    status={status}
                    component={VirtualTableRowCell}
                  />
                );
              }

              if (name === 'sortTitle') {
                return (
                  <VirtualTableRowCell
                    key={name}
                    className={styles[name]}
                  >
                    <Link
                      {...linkProps}
                    >
                      {title}
                    </Link>
                  </VirtualTableRowCell>
                );
              }

              if (name === 'studio') {
                return (
                  <VirtualTableRowCell
                    key={name}
                    className={styles[name]}
                  >
                    {studio}
                  </VirtualTableRowCell>
                );
              }

              if (name === 'inCinemas') {
                return (
                  <RelativeDateCellConnector
                    key={name}
                    className={styles[name]}
                    date={inCinemas}
                    component={VirtualTableRowCell}
                  />
                );
              }

              if (name === 'physicalRelease') {
                return (
                  <RelativeDateCellConnector
                    key={name}
                    className={styles[name]}
                    date={physicalRelease}
                    component={VirtualTableRowCell}
                  />
                );
              }

              if (name === 'genres') {
                const joinedGenres = genres.join(', ');

                return (
                  <VirtualTableRowCell
                    key={name}
                    className={styles[name]}
                  >
                    <span title={joinedGenres}>
                      {joinedGenres}
                    </span>
                  </VirtualTableRowCell>
                );
              }

              if (name === 'ratings') {
                return (
                  <VirtualTableRowCell
                    key={name}
                    className={styles[name]}
                  >
                    <HeartRating
                      rating={ratings.value}
                    />
                  </VirtualTableRowCell>
                );
              }

              if (name === 'certification') {
                return (
                  <VirtualTableRowCell
                    key={name}
                    className={styles[name]}
                  >
                    {certification}
                  </VirtualTableRowCell>
                );
              }

              return null;
            })
          }

          <AddNewMovieModal
            isOpen={isNewAddMovieModalOpen && !isExistingMovie}
            tmdbId={tmdbId}
            title={title}
            year={year}
            overview={overview}
            folder={folder}
            images={images}
            onModalClose={this.onAddMovieModalClose}
          />
        </VirtualTableRow>
      </div>
    );
  }
}

AddListMovieRow.propTypes = {
  style: PropTypes.object.isRequired,
  tmdbId: PropTypes.number.isRequired,
  status: PropTypes.string.isRequired,
  title: PropTypes.string.isRequired,
  year: PropTypes.number.isRequired,
  overview: PropTypes.string.isRequired,
  folder: PropTypes.string.isRequired,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  titleSlug: PropTypes.string.isRequired,
  studio: PropTypes.string,
  inCinemas: PropTypes.string,
  physicalRelease: PropTypes.string,
  genres: PropTypes.arrayOf(PropTypes.string).isRequired,
  ratings: PropTypes.object.isRequired,
  certification: PropTypes.string,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  isExistingMovie: PropTypes.bool.isRequired
};

AddListMovieRow.defaultProps = {
  genres: [],
  tags: []
};

export default AddListMovieRow;
