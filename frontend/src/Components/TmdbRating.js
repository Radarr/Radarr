import PropTypes from 'prop-types';
import React, { PureComponent } from 'react';
import styles from './TmdbRating.css';

class TmdbRating extends PureComponent {

  //
  // Render

  render() {

    const {
      ratings,
      hideIcon,
      iconSize
    } = this.props;

    const tmdbImage = 'data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCAxOTAuMjQgODEuNTIiPjxkZWZzPjxsaW5lYXJHcmFkaWVudCBpZD0iYSIgeTE9IjQwLjc2IiB4Mj0iMTkwLjI0IiB5Mj0iNDAuNzYiIGdyYWRpZW50VW5pdHM9InVzZXJTcGFjZU9uVXNlIj48c3RvcCBvZmZzZXQ9IjAiIHN0b3AtY29sb3I9IiM5MGNlYTEiLz48c3RvcCBvZmZzZXQ9Ii41NiIgc3RvcC1jb2xvcj0iIzNjYmVjOSIvPjxzdG9wIG9mZnNldD0iMSIgc3RvcC1jb2xvcj0iIzAwYjNlNSIvPjwvbGluZWFyR3JhZGllbnQ+PC9kZWZzPjxwYXRoIGQ9Ik0xMDUuNjcgMzYuMDZoNjYuOWExNy42NyAxNy42NyAwIDAwMTcuNjctMTcuNjZBMTcuNjcgMTcuNjcgMCAwMDE3Mi41Ny43M2gtNjYuOUExNy42NyAxNy42NyAwIDAwODggMTguNGExNy42NyAxNy42NyAwIDAwMTcuNjcgMTcuNjZ6bS04OCA0NWg3Ni45YTE3LjY3IDE3LjY3IDAgMDAxNy42Ny0xNy42NiAxNy42NyAxNy42NyAwIDAwLTE3LjY3LTE3LjY3aC03Ni45QTE3LjY3IDE3LjY3IDAgMDAwIDYzLjRhMTcuNjcgMTcuNjcgMCAwMDE3LjY3IDE3LjY2em0tNy4yNi00NS42NGg3LjhWNi45MmgxMC4xVjBoLTI4djYuOWgxMC4xem0yOC4xIDBoNy44VjguMjVoLjFsOSAyNy4xNWg2bDkuMy0yNy4xNWguMVYzNS40aDcuOFYwSDY2Ljc2bC04LjIgMjMuMWgtLjFMNTAuMzEgMGgtMTEuOHptMTEzLjkyIDIwLjI1YTE1LjA3IDE1LjA3IDAgMDAtNC41Mi01LjUyIDE4LjU3IDE4LjU3IDAgMDAtNi42OC0zLjA4IDMzLjU0IDMzLjU0IDAgMDAtOC4wNy0xaC0xMS43djM1LjRoMTIuNzVhMjQuNTggMjQuNTggMCAwMDcuNTUtMS4xNSAxOS4zNCAxOS4zNCAwIDAwNi4zNS0zLjMyIDE2LjI3IDE2LjI3IDAgMDA0LjM3LTUuNSAxNi45MSAxNi45MSAwIDAwMS42My03LjU4IDE4LjUgMTguNSAwIDAwLTEuNjgtOC4yNXpNMTQ1IDY4LjZhOC44IDguOCAwIDAxLTIuNjQgMy40IDEwLjcgMTAuNyAwIDAxLTQgMS44MiAyMS41NyAyMS41NyAwIDAxLTUgLjU1aC00LjA1di0yMWg0LjZhMTcgMTcgMCAwMTQuNjcuNjMgMTEuNjYgMTEuNjYgMCAwMTMuODggMS44N0E5LjE0IDkuMTQgMCAwMTE0NSA1OWE5Ljg3IDkuODcgMCAwMTEgNC41MiAxMS44OSAxMS44OSAwIDAxLTEgNS4wOHptNDQuNjMtLjEzYTggOCAwIDAwLTEuNTgtMi42MiA4LjM4IDguMzggMCAwMC0yLjQyLTEuODUgMTAuMzEgMTAuMzEgMCAwMC0zLjE3LTF2LS4xYTkuMjIgOS4yMiAwIDAwNC40Mi0yLjgyIDcuNDMgNy40MyAwIDAwMS42OC01IDguNDIgOC40MiAwIDAwLTEuMTUtNC42NSA4LjA5IDguMDkgMCAwMC0zLTIuNzIgMTIuNTYgMTIuNTYgMCAwMC00LjE4LTEuMyAzMi44NCAzMi44NCAwIDAwLTQuNjItLjMzaC0xMy4ydjM1LjRoMTQuNWEyMi40MSAyMi40MSAwIDAwNC43Mi0uNSAxMy41MyAxMy41MyAwIDAwNC4yOC0xLjY1IDkuNDIgOS40MiAwIDAwMy4xLTMgOC41MiA4LjUyIDAgMDAxLjItNC42OCA5LjM5IDkuMzkgMCAwMC0uNTUtMy4xOHptLTE5LjQyLTE1Ljc1aDUuM2ExMCAxMCAwIDAxMS44NS4xOCA2LjE4IDYuMTggMCAwMTEuNy41NyAzLjM5IDMuMzkgMCAwMTEuMjIgMS4xMyAzLjIyIDMuMjIgMCAwMS40OCAxLjgyIDMuNjMgMy42MyAwIDAxLS40MyAxLjggMy40IDMuNCAwIDAxLTEuMTIgMS4yIDQuOTIgNC45MiAwIDAxLTEuNTguNjUgNy41MSA3LjUxIDAgMDEtMS43Ny4yaC01LjY1em0xMS43MiAyMGEzLjkgMy45IDAgMDEtMS4yMiAxLjMgNC42NCA0LjY0IDAgMDEtMS42OC43IDguMTggOC4xOCAwIDAxLTEuODIuMmgtN3YtOGg1LjlhMTUuMzUgMTUuMzUgMCAwMTIgLjE1IDguNDcgOC40NyAwIDAxMi4wNS41NSA0IDQgMCAwMTEuNTcgMS4xOCAzLjExIDMuMTEgMCAwMS42MyAyIDMuNzEgMy43MSAwIDAxLS40MyAxLjkyeiIgZmlsbD0idXJsKCNhKSIvPjwvc3ZnPg==';

    const rating = ratings.tmdb;

    let ratingString = '0%';

    if (rating) {
      ratingString = `${rating.value * 10}%`;
    }

    return (
      <span title={`${rating.votes} votes`}>
        {
          !hideIcon &&
            <img
              className={styles.image}
              src={tmdbImage}
              style={{
                height: `${iconSize}px`
              }}
            />
        }

        {ratingString}
      </span>
    );
  }
}

TmdbRating.propTypes = {
  ratings: PropTypes.object.isRequired,
  iconSize: PropTypes.number.isRequired,
  hideIcon: PropTypes.bool
};

TmdbRating.defaultProps = {
  iconSize: 14
};

export default TmdbRating;
