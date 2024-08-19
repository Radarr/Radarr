import { IconDefinition } from '@fortawesome/free-regular-svg-icons';
import React, { useMemo } from 'react';
import { useSelector } from 'react-redux';
import { icons } from 'Helpers/Props';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import dimensions from 'Styles/Variables/dimensions';
import QualityProfile from 'typings/QualityProfile';
import UiSettings from 'typings/Settings/UiSettings';
import formatDateTime from 'Utilities/Date/formatDateTime';
import getRelativeDate from 'Utilities/Date/getRelativeDate';
import formatBytes from 'Utilities/Number/formatBytes';
import MovieIndexOverviewInfoRow from './MovieIndexOverviewInfoRow';
import styles from './MovieIndexOverviewInfo.css';

interface RowProps {
  name: string;
  showProp: string;
  valueProp: string;
}

interface RowInfoProps {
  title: string;
  iconName: IconDefinition;
  label: string;
}

interface MovieIndexOverviewInfoProps {
  height: number;
  showStudio: boolean;
  showMonitored: boolean;
  showQualityProfile: boolean;
  showAdded: boolean;
  showPath: boolean;
  showSizeOnDisk: boolean;
  monitored: boolean;
  studio?: string;
  qualityProfile?: QualityProfile;
  added?: string;
  path: string;
  sizeOnDisk?: number;
  sortKey: string;
}

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
    valueProp: 'qualityProfile',
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

function getInfoRowProps(
  row: RowProps,
  props: MovieIndexOverviewInfoProps,
  uiSettings: UiSettings
): RowInfoProps | null {
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
      label: props.studio ?? '',
    };
  }

  if (name === 'qualityProfileId' && !!props.qualityProfile?.name) {
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
      label:
        getRelativeDate(added, shortDateFormat, showRelativeDates, {
          timeFormat,
          timeForToday: true,
        }) ?? '',
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
    const { sizeOnDisk = 0 } = props;

    return {
      title: 'Size on Disk',
      iconName: icons.DRIVE,
      label: formatBytes(sizeOnDisk),
    };
  }

  return null;
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
        // eslint-disable-next-line @typescript-eslint/ban-ts-comment
        // @ts-ignore ts(7053)
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

        if (infoRowProps == null) {
          return null;
        }

        return <MovieIndexOverviewInfoRow key={row.name} {...infoRowProps} />;
      })}
    </div>
  );
}

export default MovieIndexOverviewInfo;
