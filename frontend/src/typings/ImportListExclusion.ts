import ModelBase from 'App/ModelBase';

export default interface ImportListExclusion extends ModelBase {
  tmdbId: number;
  movieTitle: string;
  movieYear: number;
}
