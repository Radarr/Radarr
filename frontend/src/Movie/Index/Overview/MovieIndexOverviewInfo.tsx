import React, { useMemo } from 'react';
import { useSelector } from 'react-redux';
import { icons } from 'Helpers/Props';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import dimensions from 'Styles/Variables/dimensions';
import formatDateTime from 'Utilities/Date/formatDateTime';
import getRelativeDate from 'Utilities/Date/getRelativeDate';
import formatBytes from 'Utilities/Number/formatBytes';
import MovieIndexOverviewInfoRow from './MovieIndexOverviewInfoRow';
import styles from './MovieIndexOverviewInfo.css';

const infoRowHeight = parseInt(dimensions.movieIndexOverviewInfoRowHeight);

const rows = [
  {
    name: 'monitored',
    showProp: 'showMonitored',
    valueProp: 'monitored',
  },
  {
    name: 'studio',
    showProp: 'showStudio',
    valueProp: 'studio',
  },
  {
    name: 'qualityProfileId',
    showProp: 'showQualityProfile',
    valueProp: 'qualityProfileId',
  },
  {
    name: 'added',
    showProp: 'showAdded',
    valueProp: 'added',
  },
  {
    name: 'path',
    showProp: 'showPath',
    valueProp: 'path',
  },
  {
    name: 'sizeOnDisk',
    showProp: 'showSizeOnDisk',
    valueProp: 'sizeOnDisk',
  },
];

function getInfoRowProps(row, props, uiSettings) {
  const { name } = row;

  if (name === 'monitored') {
    const monitoredText = props.monitored ? 'Monitored' : 'Unmonitored';

    return {
      title: monitoredText,
      iconName: props.monitored ? icons.MONITORED : icons.UNMONITORED,
      label: monitoredText,
    };
  }

  if (name === 'studio') {
    return {
      title: 'Studio',
      iconName: icons.STUDIO,
      label: props.studio,
    };
  }

  if (name === 'qualityProfileId') {
    return {
      title: 'Quality Profile',
      iconName: icons.PROFILE,
      label: props.qualityProfile.name,
    };
  }

  if (name === 'added') {
    const added = props.added;
    const { showRelativeDates, shortDateFormat, longDateFormat, timeFormat } =
      uiSettings;

    return {
      title: `Added: ${formatDateTime(added, longDateFormat, timeFormat)}`,
      iconName: icons.ADD,
      label: getRelativeDate(added, shortDateFormat, showRelativeDates, {
        timeFormat,
        timeForToday: true,
      }),
    };
  }

  if (name === 'path') {
    return {
      title: 'Path',
      iconName: icons.FOLDER,
      label: props.path,
    };
  }

  if (name === 'sizeOnDisk') {
    return {
      title: 'Size on Disk',
      iconName: icons.DRIVE,
      label: formatBytes(props.sizeOnDisk),
    };
  }
}

interface MovieIndexOverviewInfoProps {
  height: number;
  showMonitored: boolean;
  showQualityProfile: boolean;
  showAdded: boolean;
  showPath: boolean;
  showSizeOnDisk: boolean;
  monitored: boolean;
  qualityProfile: object;
  added?: string;
  path: string;
  sizeOnDisk?: number;
  sortKey: string;
}

function MovieIndexOverviewInfo(props: MovieIndexOverviewInfoProps) {
  const height = props.height;

  const uiSettings = useSelector(createUISettingsSelector());

  let shownRows = 1;
  const maxRows = Math.floor(height / (infoRowHeight + 4));

  const rowInfo = useMemo(() => {
    return rows.map((row) => {
      const { name, showProp, valueProp } = row;

      const isVisible =
        props[valueProp] != null && (props[showProp] || props.sortKey === name);

      return {
        ...row,
        isVisible,
      };
    });
  }, [props]);

  return (
    <div className={styles.infos}>
      {rowInfo.map((row) => {
        if (!row.isVisible) {
          return null;
        }

        if (shownRows >= maxRows) {
          return null;
        }

        shownRows++;

        const infoRowProps = getInfoRowProps(row, props, uiSettings);

        return <MovieIndexOverviewInfoRow key={row.name} {...infoRowProps} />;
      })}
    </div>
  );
}

export default MovieIndexOverviewInfo;
