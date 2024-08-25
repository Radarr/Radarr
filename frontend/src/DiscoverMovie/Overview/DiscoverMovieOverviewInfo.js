import _ from 'lodash';
import PropTypes from 'prop-types';
import React from 'react';
import { icons } from 'Helpers/Props';
import dimensions from 'Styles/Variables/dimensions';
import translate from 'Utilities/String/translate';
import DiscoverMovieOverviewInfoRow from './DiscoverMovieOverviewInfoRow';
import styles from './DiscoverMovieOverviewInfo.css';

const infoRowHeight = parseInt(dimensions.movieIndexOverviewInfoRowHeight);

const rows = [
  {
    name: 'year',
    showProp: 'showYear',
    valueProp: 'year'
  },
  {
    name: 'studio',
    showProp: 'showStudio',
    valueProp: 'studio'
  },
  {
    name: 'genres',
    showProp: 'showGenres',
    valueProp: 'genres'
  },
  {
    name: 'tmdbRating',
    showProp: 'showTmdbRating',
    valueProp: 'ratings.tmdb.value'
  },
  {
    name: 'imdbRating',
    showProp: 'showImdbRating',
    valueProp: 'ratings.imdb.value'
  },
  {
    name: 'certification',
    showProp: 'showCertification',
    valueProp: 'certification'
  }
];

function isVisible(row, props) {
  const {
    name,
    showProp,
    valueProp
  } = row;

  return _.has(props, valueProp) && (_.get(props, showProp) || props.sortKey === name);
}

function getInfoRowProps(row, props) {
  const { name } = row;

  if (name === 'year') {
    return {
      title: translate('Year'),
      iconName: icons.CALENDAR,
      label: props.year
    };
  }

  if (name === 'studio') {
    return {
      title: translate('Studio'),
      iconName: icons.STUDIO,
      label: props.studio
    };
  }

  if (name === 'genres') {
    return {
      title: translate('Genres'),
      iconName: icons.GENRE,
      label: props.genres.slice(0, 2).join(', ')
    };
  }

  if (name === 'tmdbRating' && !!props.ratings.tmdb) {
    return {
      title: translate('TmdbRating'),
      iconName: icons.HEART,
      label: `${(props.ratings.tmdb.value * 10).toFixed()}%`
    };
  }

  if (name === 'imdbRating' && !!props.ratings.imdb) {
    return {
      title: translate('ImdbRating'),
      iconName: icons.IMDB,
      label: `${(props.ratings.imdb.value).toFixed(1)}`
    };
  }

  if (name === 'certification') {
    return {
      title: translate('Certification'),
      iconName: icons.FILM,
      label: props.certification
    };
  }
}

function DiscoverMovieOverviewInfo(props) {
  const {
    height
  } = props;

  let shownRows = 1;
  const maxRows = Math.floor(height / (infoRowHeight + 4));

  return (
    <div className={styles.infos}>
      {
        rows.map((row) => {
          if (!isVisible(row, props)) {
            return null;
          }

          if (shownRows >= maxRows) {
            return null;
          }

          shownRows++;

          const infoRowProps = getInfoRowProps(row, props);

          return (
            <DiscoverMovieOverviewInfoRow
              key={row.name}
              {...infoRowProps}
            />
          );
        })
      }
    </div>
  );
}

DiscoverMovieOverviewInfo.propTypes = {
  height: PropTypes.number.isRequired,
  showYear: PropTypes.bool.isRequired,
  showStudio: PropTypes.bool.isRequired,
  showGenres: PropTypes.bool.isRequired,
  showTmdbRating: PropTypes.bool.isRequired,
  showImdbRating: PropTypes.bool.isRequired,
  showCertification: PropTypes.bool.isRequired,
  studio: PropTypes.string,
  year: PropTypes.number,
  certification: PropTypes.string,
  ratings: PropTypes.object,
  genres: PropTypes.arrayOf(PropTypes.string).isRequired,
  sortKey: PropTypes.string.isRequired
};

export default DiscoverMovieOverviewInfo;
