type ColonReplacementFormat =
  | 'delete'
  | 'dash'
  | 'spaceDash'
  | 'spaceDashSpace'
  | 'smart';

export default interface NamingConfig {
  renameMovies: boolean;
  replaceIllegalCharacters: boolean;
  colonReplacementFormat: ColonReplacementFormat;
  standardMovieFormat: string;
  movieFolderFormat: string;
}
