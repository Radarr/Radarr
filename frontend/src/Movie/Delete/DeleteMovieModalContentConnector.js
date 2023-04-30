import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { deleteMovie, setDeleteOption } from 'Store/Actions/movieActions';
import createMovieSelector from 'Store/Selectors/createMovieSelector';
import DeleteMovieModalContent from './DeleteMovieModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.movies.deleteOptions,
    createMovieSelector(),
    (deleteOptions, movie) => {
      return {
        ...movie,
        deleteOptions
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onDeleteOptionChange(option) {
      dispatch(
        setDeleteOption({
          [option.name]: option.value
        })
      );
    },

    onDeletePress(deleteFiles, addImportExclusion) {
      dispatch(
        deleteMovie({
          id: props.movieId,
          deleteFiles,
          addImportExclusion
        })
      );

      props.onModalClose(true);
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(DeleteMovieModalContent);
