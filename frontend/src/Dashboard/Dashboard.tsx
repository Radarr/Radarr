import React, { useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import { MediaTypeStatistics } from 'App/State/DashboardAppState';
import Icon, { IconName } from 'Components/Icon';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import { icons } from 'Helpers/Props';
import { fetchDashboard } from 'Store/Actions/dashboardActions';
import formatBytes from 'Utilities/Number/formatBytes';
import translate from 'Utilities/String/translate';
import styles from './Dashboard.css';

function createDashboardSelector() {
  return createSelector(
    (state: AppState) => state.dashboard,
    (dashboard) => {
      return dashboard;
    }
  );
}

interface MediaTypeCardProps {
  title: string;
  icon: IconName;
  stats: MediaTypeStatistics;
  showDuration?: boolean;
}

function MediaTypeCard({
  title,
  icon,
  stats,
  showDuration = false,
}: MediaTypeCardProps) {
  if (!stats) {
    return null;
  }

  return (
    <div className={styles.mediaCard}>
      <div className={styles.mediaCardHeader}>
        <Icon name={icon} size={24} />
        <h3 className={styles.mediaCardTitle}>{title}</h3>
      </div>
      <div className={styles.statsRow}>
        <span className={styles.statsLabel}>{translate('Total')}</span>
        <span className={styles.statsValue}>{stats.total}</span>
      </div>
      <div className={styles.statsRow}>
        <span className={styles.statsLabel}>{translate('WithFiles')}</span>
        <span className={styles.statsValue}>{stats.withFiles}</span>
      </div>
      <div className={styles.statsRow}>
        <span className={styles.statsLabel}>{translate('Missing')}</span>
        <span className={styles.statsValue}>{stats.missing}</span>
      </div>
      <div className={styles.statsRow}>
        <span className={styles.statsLabel}>{translate('Monitored')}</span>
        <span className={styles.statsValue}>{stats.monitored}</span>
      </div>
      <div className={styles.statsRow}>
        <span className={styles.statsLabel}>{translate('Unmonitored')}</span>
        <span className={styles.statsValue}>{stats.unmonitored}</span>
      </div>
      <div className={styles.statsRow}>
        <span className={styles.statsLabel}>{translate('SizeOnDisk')}</span>
        <span className={styles.statsValue}>
          {formatBytes(stats.sizeOnDisk)}
        </span>
      </div>
      {showDuration && stats.totalDurationMinutes > 0 && (
        <div className={styles.statsRow}>
          <span className={styles.statsLabel}>
            {translate('TotalDuration')}
          </span>
          <span className={styles.statsValue}>
            {Math.floor(stats.totalDurationMinutes / 60)}h{' '}
            {stats.totalDurationMinutes % 60}m
          </span>
        </div>
      )}
    </div>
  );
}

function Dashboard() {
  const dispatch = useDispatch();
  const { isFetching, item } = useSelector(createDashboardSelector());

  useEffect(() => {
    dispatch(fetchDashboard());
  }, [dispatch]);

  return (
    <PageContent title={translate('Dashboard')}>
      <PageContentBody>
        {isFetching ? <LoadingIndicator /> : null}

        {!isFetching && item ? (
          <div className={styles.dashboard}>
            <div className={styles.statsGrid}>
              <MediaTypeCard
                title={translate('Movies')}
                icon={icons.MOVIE_FILE}
                stats={item.movies}
              />
              <MediaTypeCard
                title={translate('Books')}
                icon={icons.BOOK}
                stats={item.books}
              />
              <MediaTypeCard
                title={translate('Audiobooks')}
                icon={icons.AUDIOBOOK}
                stats={item.audiobooks}
                showDuration={true}
              />
            </div>

            <div className={styles.totalCard}>
              <h3 className={styles.totalCardTitle}>
                {translate('TotalLibrarySize')}
              </h3>
              <span className={styles.totalValue}>
                {formatBytes(item.totalSizeOnDisk)}
              </span>
            </div>
          </div>
        ) : null}
      </PageContentBody>
    </PageContent>
  );
}

export default Dashboard;
