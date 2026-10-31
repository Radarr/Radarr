interface MovieVersion {
  id: number;
  movieId: number;
  qualityProfileId: number;
  path: string;
  movieFileId: number;
  hasFile: boolean;
  isPrimary: boolean;
}

export default MovieVersion;
