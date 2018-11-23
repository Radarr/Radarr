import _ from 'lodash';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createAllMoviesSelector from 'Store/Selectors/createAllMoviesSelector';
import createTagsSelector from 'Store/Selectors/createTagsSelector';
import TagsModalContent from './TagsModalContent';

function createMapStateToProps() {
  return createSelector(
    (state, { seriesIds }) => seriesIds,
    createAllMoviesSelector(),
    createTagsSelector(),
    (seriesIds, allMovies, tagList) => {
      const series = _.intersectionWith(allMovies, seriesIds, (s, id) => {
        return s.id === id;
      });

      const seriesTags = _.uniq(_.concat(..._.map(series, 'tags')));

      return {
        seriesTags,
        tagList
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onAction() {
      // Do something
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(TagsModalContent);
