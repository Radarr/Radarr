import PropTypes from 'prop-types';
import React from 'react';
import InteractiveSearchConnector from 'InteractiveSearch/InteractiveSearchConnector';

function SeasonInteractiveSearchModalContent(props) {
  const {
    movieId
  } = props;

  return (
    <div>
      <InteractiveSearchConnector
        searchPayload={{
          movieId
        }}
      />
    </div>
  );
}

SeasonInteractiveSearchModalContent.propTypes = {
  movieId: PropTypes.number.isRequired
};

export default SeasonInteractiveSearchModalContent;
