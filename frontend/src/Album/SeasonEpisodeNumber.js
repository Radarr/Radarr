import PropTypes from 'prop-types';
import React from 'react';
import EpisodeNumber from './EpisodeNumber';

function SeasonEpisodeNumber(props) {
  const {
    airDate,
    artistType,
    ...otherProps
  } = props;

  if (artistType === 'daily' && airDate) {
    return (
      <span>{airDate}</span>
    );
  }

  return (
    <EpisodeNumber
      seriesType={artistType}
      showSeasonNumber={true}
      {...otherProps}
    />
  );
}

SeasonEpisodeNumber.propTypes = {
  airDate: PropTypes.string,
  artistType: PropTypes.string
};

export default SeasonEpisodeNumber;
