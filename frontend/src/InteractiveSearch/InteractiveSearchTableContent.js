import PropTypes from 'prop-types';
import React from 'react';
import InteractiveSearchConnector from 'InteractiveSearch/InteractiveSearchConnector';

function InteractiveSearchTableContent(props) {
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

InteractiveSearchTableContent.propTypes = {
  movieId: PropTypes.number.isRequired
};

export default InteractiveSearchTableContent;
