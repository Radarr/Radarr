import React from 'react';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';
import translate from 'Utilities/String/translate';

function MovieMinimumAvailabilityPopoverContent() {
  return (
    <DescriptionList>
      <DescriptionListItem
        title={translate('Announced')}
        data={translate('AnnouncedMovieAvailabilityDescription')}
      />

      <DescriptionListItem
        title={translate('InCinemas')}
        data={translate('InCinemasMovieAvailabilityDescription')}
      />

      <DescriptionListItem
        title={translate('Released')}
        data={translate('ReleasedMovieAvailabilityDescription')}
      />
    </DescriptionList>
  );
}

export default MovieMinimumAvailabilityPopoverContent;
