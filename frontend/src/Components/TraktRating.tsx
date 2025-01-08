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
    'data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCA0OCA0OCI+PGRlZnM+PHJhZGlhbEdyYWRpZW50IGlkPSJhIiBjeD0iNDguNDYiIGN5PSItLjk1IiByPSI2NC44NCIgZng9IjQ4LjQ2IiBmeT0iLS45NSIgZ3JhZGllbnRVbml0cz0idXNlclNwYWNlT25Vc2UiPjxzdG9wIG9mZnNldD0iMCIgc3RvcC1jb2xvcj0iIzlmNDJjNiIvPjxzdG9wIG9mZnNldD0iLjI3IiBzdG9wLWNvbG9yPSIjYTA0MWMzIi8+PHN0b3Agb2Zmc2V0PSIuNDIiIHN0b3AtY29sb3I9IiNhNDNlYmIiLz48c3RvcCBvZmZzZXQ9Ii41MyIgc3RvcC1jb2xvcj0iI2FhMzlhZCIvPjxzdG9wIG9mZnNldD0iLjY0IiBzdG9wLWNvbG9yPSIjYjQzMzlhIi8+PHN0b3Agb2Zmc2V0PSIuNzMiIHN0b3AtY29sb3I9IiNjMDJiODEiLz48c3RvcCBvZmZzZXQ9Ii44MiIgc3RvcC1jb2xvcj0iI2NmMjA2MSIvPjxzdG9wIG9mZnNldD0iLjkiIHN0b3AtY29sb3I9IiNlMTE0M2MiLz48c3RvcCBvZmZzZXQ9Ii45NyIgc3RvcC1jb2xvcj0iI2Y1MDYxMyIvPjxzdG9wIG9mZnNldD0iMSIgc3RvcC1jb2xvcj0icmVkIi8+PC9yYWRpYWxHcmFkaWVudD48L2RlZnM+PHBhdGggZmlsbD0idXJsKCNhKSIgZD0iTTQ4IDExLjI2djI1LjQ3QzQ4IDQyLjk1IDQyLjk1IDQ4IDM2LjczIDQ4SDExLjI2QzUuMDQgNDggMCA0Mi45NSAwIDM2LjczVjExLjI2QzAgNS4wNCA1LjA0IDAgMTEuMjYgMGgyNS40N2MzLjMyIDAgNi4zIDEuNDMgOC4zNyAzLjcyLjQ3LjUyLjg5IDEuMDggMS4yNSAxLjY4LjE4LjI5LjM0LjU5LjUuODkuMzMuNjguNiAxLjM5Ljc5IDIuMTQuMS4zNy4xOC43Ni4yMyAxLjE1LjA5LjU0LjEzIDEuMTEuMTMgMS42OFoiLz48cGF0aCBmaWxsPSIjZmZmIiBkPSJtMTMuNjIgMTcuOTcgNy45MiA3LjkyIDEuNDctMS40Ny03LjkyLTcuOTItMS40NyAxLjQ3Wm0xNC4zOSAxNC40IDEuNDctMS40Ni0yLjE2LTIuMTZMNDcuNjQgOC40M2MtLjE5LS43NS0uNDYtMS40Ni0uNzktMi4xNEwyNC4zOSAyOC43NWwzLjYyIDMuNjJabS0xNS4wOS0xMy43LTEuNDYgMS40NiAxNC40IDE0LjQgMS40Ni0xLjQ3TDIzIDI4Ljc1IDQ2LjM1IDUuNGMtLjM2LS42LS43OC0xLjE2LTEuMjUtMS42OEwyMS41NCAyNy4yOGwtOC42Mi04LjYxWm0zNC45NS05LjA5TDI4LjcgMjguNzVsMS40NyAxLjQ2TDQ4IDEyLjM4di0xLjEyYzAtLjU3LS4wNC0xLjE0LS4xMy0xLjY4Wk0yNS4xNiAyMi4yN2wtNy45Mi03LjkyLTEuNDcgMS40NyA3LjkyIDcuOTIgMS40Ny0xLjQ3Wm0xNi4xNiAxMi44NWMwIDMuNDItMi43OCA2LjItNi4yIDYuMkgxMi44OGMtMy40MiAwLTYuMi0yLjc4LTYuMi02LjJWMTIuODhjMC0zLjQyIDIuNzgtNi4yMSA2LjItNi4yMWgyMC43OFY0LjZIMTIuODhjLTQuNTYgMC04LjI4IDMuNzEtOC4yOCA4LjI4djIyLjI0YzAgNC41NiAzLjcxIDguMjggOC4yOCA4LjI4aDIyLjI0YzQuNTYgMCA4LjI4LTMuNzEgOC4yOC04LjI4di0zLjUxaC0yLjA3djMuNTFaIi8+PC9zdmc+';

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
