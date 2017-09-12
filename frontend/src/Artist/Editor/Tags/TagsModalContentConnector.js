import _ from 'lodash';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createAllArtistSelector from 'Store/Selectors/createAllArtistSelector';
import createTagsSelector from 'Store/Selectors/createTagsSelector';
import TagsModalContent from './TagsModalContent';

function createMapStateToProps() {
  return createSelector(
    (state, { artistIds }) => artistIds,
    createAllArtistSelector(),
    createTagsSelector(),
    (artistIds, allSeries, tagList) => {
      const series = _.intersectionWith(allSeries, artistIds, (s, id) => {
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
