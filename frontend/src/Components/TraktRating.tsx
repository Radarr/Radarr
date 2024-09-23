import React from 'react';
import Tooltip from 'Components/Tooltip/Tooltip';
import { kinds, tooltipPositions } from 'Helpers/Props';
import { Ratings } from 'Movie/Movie';
import translate from 'Utilities/String/translate';
import styles from './TraktRating.css';

interface TraktRatingProps {
  ratings: Ratings;
  iconSize?: number;
  hideIcon?: boolean;
}

function TraktRating(props: TraktRatingProps) {
  const { ratings, iconSize = 14, hideIcon = false } = props;

  const traktImage =
    'data:image/svg+xml;base64,PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0idXRmLTgiPz48IS0tIEdlbmVyYXRvcjogQWRvYmUgSWxsdXN0cmF0b3IgMTguMC4wLCBTVkcgRXhwb3J0IFBsdWctSW4gLiBTVkcgVmVyc2lvbjogNi4wMCBCdWlsZCAwKSAgLS0+PCFET0NUWVBFIHN2ZyBQVUJMSUMgIi0vL1czQy8vRFREIFNWRyAxLjEvL0VOIiAiaHR0cDovL3d3dy53My5vcmcvR3JhcGhpY3MvU1ZHLzEuMS9EVEQvc3ZnMTEuZHRkIj48c3ZnIHZlcnNpb249IjEuMSIgaWQ9IkxheWVyXzEiIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZyIgeG1sbnM6eGxpbms9Imh0dHA6Ly93d3cudzMub3JnLzE5OTkveGxpbmsiIHg9IjBweCIgeT0iMHB4IiAgICAgdmlld0JveD0iMCAwIDE0NC44IDE0NC44IiBlbmFibGUtYmFja2dyb3VuZD0ibmV3IDAgMCAxNDQuOCAxNDQuOCIgeG1sOnNwYWNlPSJwcmVzZXJ2ZSI+PGc+ICAgIDxwYXRoIGZpbGw9IiNFRDIyMjQiIGQ9Ik0yOS41LDExMS44YzEwLjYsMTEuNiwyNS45LDE4LjgsNDIuOSwxOC44YzguNywwLDE2LjktMS45LDI0LjMtNS4zTDU2LjMsODVMMjkuNSwxMTEuOHoiLz4gICAgPHBhdGggZmlsbD0iI0VEMjIyNCIgZD0iTTU2LjEsNjAuNkwyNS41LDkxLjFMMjEuNCw4N2wzMi4yLTMyLjJoMGwzNy42LTM3LjZjLTUuOS0yLTEyLjItMy4xLTE4LjgtMy4xYy0zMi4yLDAtNTguMywyNi4xLTU4LjMsNTguMyAgICAgICBjMCwxMy4xLDQuMywyNS4yLDExLjcsMzVsMzAuNS0zMC41bDIuMSwybDQzLjcsNDMuN2MwLjktMC41LDEuNy0xLDIuNS0xLjZMNTYuMyw3Mi43TDI3LDEwMmwtNC4xLTQuMWwzMy40LTMzLjRsMi4xLDJsNTEsNTAuOSAgICAgICBjMC44LTAuNiwxLjUtMS4zLDIuMi0xLjlsLTU1LTU1TDU2LjEsNjAuNnoiLz4gICAgPHBhdGggZmlsbD0iI0VEMUMyNCIgZD0iTTExNS43LDExMS40YzkuMy0xMC4zLDE1LTI0LDE1LTM5YzAtMjMuNC0xMy44LTQzLjUtMzMuNi01Mi44TDYwLjQsNTYuMkwxMTUuNywxMTEuNHogTTc0LjUsNjYuOGwtNC4xLTQuMSAgICAgICBsMjguOS0yOC45bDQuMSw0LjFMNzQuNSw2Ni44eiBNMTAxLjksMjcuMUw2OC42LDYwLjRsLTQuMS00LjFMOTcuOCwyM0wxMDEuOSwyNy4xeiIvPiAgICA8Zz4gICAgICAgPGc+ICAgICAgICAgIDxwYXRoIGZpbGw9IiNFRDIyMjQiIGQ9Ik03Mi40LDE0NC44QzMyLjUsMTQ0LjgsMCwxMTIuMywwLDcyLjRDMCwzMi41LDMyLjUsMCw3Mi40LDBzNzIuNCwzMi41LDcyLjQsNzIuNCAgICAgICAgICAgICBDMTQ0LjgsMTEyLjMsMTEyLjMsMTQ0LjgsNzIuNCwxNDQuOHogTTcyLjQsNy4zQzM2LjUsNy4zLDcuMywzNi41LDcuMyw3Mi40czI5LjIsNjUuMSw2NS4xLDY1LjFzNjUuMS0yOS4yLDY1LjEtNjUuMSAgICAgICAgICAgICBTMTA4LjMsNy4zLDcyLjQsNy4zeiIvPiAgICAgICA8L2c+ICAgIDwvZz48L2c+PC9zdmc+';

  const { value = 0, votes = 0 } = ratings.trakt;

  return (
    <Tooltip
      anchor={
        <span className={styles.wrapper}>
          {!hideIcon && (
            <img
              className={styles.image}
              alt={translate('TraktRating')}
              src={traktImage}
              style={{
                height: `${iconSize}px`,
              }}
            />
          )}
          {(value * 10).toFixed()}%
        </span>
      }
      tooltip={translate('CountVotes', { votes })}
      kind={kinds.INVERSE}
      position={tooltipPositions.TOP}
    />
  );
}

export default TraktRating;
