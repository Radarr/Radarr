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
    'data:image/svg+xml;base64,PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiPz4KPHN2ZyBpZD0iTGF5ZXJfMiIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIiB2aWV3Qm94PSIwIDAgNDggNDgiPgogIDxkZWZzPgogICAgPHN0eWxlPgogICAgICAuY2xzLTEgewogICAgICAgIGZpbGw6ICM5ZjQyYzY7CiAgICAgIH0KCiAgICAgIC5jbHMtMiB7CiAgICAgICAgZmlsbDogI2ZmZjsKICAgICAgfQogICAgPC9zdHlsZT4KICA8L2RlZnM+CiAgPGcgaWQ9Il94MkRfLXByb2R1Y3Rpb24iPgogICAgPGcgaWQ9ImxvZ29tYXJrLmNpcmNsZS5jb2xvciI+CiAgICAgIDxwYXRoIGlkPSJiYWNrZ3JvdW5kIiBjbGFzcz0iY2xzLTEiIGQ9Ik00OCwyNGMwLDYuNjItMi42OSwxMi42Mi03LjAzLDE2Ljk3LTQuMzQsNC4zNC0xMC4zNCw3LjAzLTE2Ljk3LDcuMDNDMTAuNzUsNDgsMCwzNy4yNSwwLDI0YzAtNi42MywyLjY5LTEyLjYzLDcuMDMtMTYuOTdDMTEuMzcsMi42OCwxNy4zNywwLDI0LDBzMTIuNjMsMi42OCwxNi45Nyw3LjAzYy4xNC4xNC4yNy4yOC40LjQyLjQ4LjUuOTQsMS4wMiwxLjM3LDEuNTYuMjEuMjYuNDEuNTIuNi43OS40My41Ny44MiwxLjE2LDEuMTgsMS43Ni4xOC4yOS4zNS41OC41MS44Ny4zNS42NC42OCwxLjI5Ljk2LDEuOTcsMS4zLDIuOTQsMi4wMSw2LjE4LDIuMDEsOS42WiIvPgogICAgICA8ZyBpZD0iY2hlY2tib3giPgogICAgICAgIDxwYXRoIGNsYXNzPSJjbHMtMiIgZD0iTTEyLjkzLDE4LjY3bC0xLjQ3LDEuNDYsMTQuNCwxNC40LDEuNDctMS40Ny00LjMyLTQuMzEsMTkuNzMtMTkuNzRjLS40My0uNTQtLjg5LTEuMDYtMS4zNy0xLjU2bC0xOS44MywxOS44My04LjYxLTguNjFaTTI4LjAyLDMyLjM3bDEuNDYtMS40Ni0yLjE1LTIuMTYsMTcuMTktMTcuMTljLS4zNi0uNi0uNzUtMS4xOS0xLjE4LTEuNzZsLTE4Ljk0LDE4Ljk1LDMuNjIsMy42MlpNMzAuMTgsMzAuMjFsMTUuODEtMTUuODFjLS4yOC0uNjgtLjYxLTEuMzMtLjk2LTEuOTdsLTE2LjMyLDE2LjMyLDEuNDcsMS40NlpNMTMuNjIsMTcuOTdsNy45Miw3LjkyLDEuNDctMS40Ny03LjkyLTcuOTItMS40NywxLjQ3Wk0yNS4xNywyMi4yN2wtNy45Mi03LjkyLTEuNDcsMS40Nyw3LjkyLDcuOTIsMS40Ny0xLjQ3Wk0yNCw0MS4zMmMtOS41NSwwLTE3LjMyLTcuNzctMTcuMzItMTcuMzJTMTQuNDUsNi42NywyNCw2LjY3YzIuNiwwLDUuMTEuNTYsNy40NCwxLjY4bC44OS0xLjg3Yy0yLjYxLTEuMjUtNS40Mi0xLjg4LTguMzMtMS44OEMxMy4zMSw0LjYsNC42MSwxMy4zLDQuNjEsMjRzOC43LDE5LjQsMTkuNCwxOS40YzcuNjQsMCwxNC41OS00LjUxLDE3LjcxLTExLjQ4bC0xLjg5LS44NWMtMi43OSw2LjIzLTksMTAuMjYtMTUuODIsMTAuMjZaIi8+CiAgICAgIDwvZz4KICAgIDwvZz4KICA8L2c+Cjwvc3ZnPg==';

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
