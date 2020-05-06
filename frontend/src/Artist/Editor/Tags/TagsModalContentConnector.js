import _ from 'lodash';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createAllArtistSelector from 'Store/Selectors/createAllArtistSelector';
import createTagsSelector from 'Store/Selectors/createTagsSelector';
import TagsModalContent from './TagsModalContent';

function createMapStateToProps() {
  return createSelector(
    (state, { authorIds }) => authorIds,
    createAllArtistSelector(),
    createTagsSelector(),
    (authorIds, allArtists, tagList) => {
      const artist = _.intersectionWith(allArtists, authorIds, (s, id) => {
        return s.id === id;
      });

      const artistTags = _.uniq(_.concat(..._.map(artist, 'tags')));

      return {
        artistTags,
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
