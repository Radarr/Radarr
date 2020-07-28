import PropTypes from 'prop-types';
import React from 'react';
import QueueDetails from 'Activity/Queue/QueueDetails';
import CircularProgressBar from 'Components/CircularProgressBar';
import colors from 'Styles/Variables/colors';

function CalendarEventQueueDetails(props) {
  const {
    title,
    size,
    sizeleft,
    estimatedCompletionTime,
    status,
    errorMessage
  } = props;

  const progress = (100 - sizeleft / size * 100);

  return (
    <QueueDetails
      title={title}
      size={size}
      sizeleft={sizeleft}
      estimatedCompletionTime={estimatedCompletionTime}
      status={status}
      errorMessage={errorMessage}
      progressBar={
        <div title={`Movie is downloading - ${progress.toFixed(1)}% ${title}`}>
          <CircularProgressBar
            progress={progress}
            size={20}
            strokeWidth={2}
            strokeColor={colors.purple}
          />
        </div>
      }
    />
  );
}

CalendarEventQueueDetails.propTypes = {
  title: PropTypes.string.isRequired,
  size: PropTypes.number.isRequired,
  sizeleft: PropTypes.number.isRequired,
  estimatedCompletionTime: PropTypes.string,
  status: PropTypes.string.isRequired,
  errorMessage: PropTypes.string
};

export default CalendarEventQueueDetails;
