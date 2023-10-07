import AppSectionState from 'App/State/AppSectionState';
import MovieCollection from 'typings/MovieCollection';

interface MovieCollectionAppState extends AppSectionState<MovieCollection> {
  itemMap: Record<number, number>;
}

export default MovieCollectionAppState;
