import PropTypes from 'prop-types';
import React from 'react';
import EpisodeNumber from './EpisodeNumber';

function SeasonEpisodeNumber(props) {
  const {
    airDate,
    authorType,
    ...otherProps
  } = props;

  if (authorType === 'daily' && airDate) {
    return (
      <span>{airDate}</span>
    );
  }

  return (
    <EpisodeNumber
      seriesType={authorType}
      showSeasonNumber={true}
      {...otherProps}
    />
  );
}

SeasonEpisodeNumber.propTypes = {
  airDate: PropTypes.string,
  authorType: PropTypes.string
};

export default SeasonEpisodeNumber;
