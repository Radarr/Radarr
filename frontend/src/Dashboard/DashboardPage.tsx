import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import Movie from 'Movie/Movie';
import { moveDashboardWidget } from 'Personalization/preferences';
import { setDashboardEditing, setDashboardWidgets } from 'Store/Actions/personalizedUiActions';
import { fetchQueueDetails } from 'Store/Actions/queueActions';
import { fetchDiskSpace, fetchHealth } from 'Store/Actions/systemActions';
import formatBytes from 'Utilities/Number/formatBytes';
import styles from './DashboardPage.css';

const widgetTitles: Record<string, string> = {
  recentlyAdded: 'Recently added', upcomingReleases: 'Upcoming releases', missingMonitored: 'Missing monitored movies', activeDownloads: 'Active downloads', attention: 'Movies requiring attention', libraryStatistics: 'Library statistics', diskSpace: 'Disk-space summary', calendarPreview: 'Calendar preview',
};

function DashboardPage() {
  const dispatch = useDispatch();
  const movies = useSelector((state: AppState) => state.movies.items) as Movie[];
  const queue = useSelector((state: AppState) => state.queue.details.items);
  const diskSpace = useSelector((state: AppState) => state.system.diskSpace.items);
  const health = useSelector((state: AppState) => state.system.health.items);
  const { widgets, isEditing } = useSelector((state: AppState) => state.personalizedUi.dashboard);
  const [draggedId, setDraggedId] = useState<string | null>(null);

  useEffect(() => {
    dispatch(fetchDiskSpace());
    dispatch(fetchHealth());
    dispatch(fetchQueueDetails());
  }, [dispatch]);

  const orderedMovies = useMemo(() => [...movies].sort((a, b) => Date.parse(b.added) - Date.parse(a.added)), [movies]);
  const upcoming = useMemo(() => movies.filter((movie) => movie.releaseDate && Date.parse(movie.releaseDate) >= Date.now()).sort((a, b) => Date.parse(a.releaseDate!) - Date.parse(b.releaseDate!)), [movies]);
  const missing = movies.filter((movie) => movie.monitored && movie.isAvailable && !movie.hasFile);
  const attention = movies.filter((movie) => movie.status === 'deleted' || (movie.monitored && !movie.hasFile));

  const move = useCallback((id: string, offset: number) => {
    const next = moveDashboardWidget(widgets, id, offset);
    dispatch(setDashboardWidgets({ widgets: next }));
  }, [dispatch, widgets]);

  const moveBefore = useCallback((id: string, targetId: string) => {
    if (id === targetId) return;
    const next = widgets.filter((widget) => widget.id !== id);
    const item = widgets.find((widget) => widget.id === id);
    const target = next.findIndex((widget) => widget.id === targetId);
    if (item) { next.splice(target, 0, item); dispatch(setDashboardWidgets({ widgets: next })); }
  }, [dispatch, widgets]);

  const toggle = useCallback((id: string) => dispatch(setDashboardWidgets({ widgets: widgets.map((widget) => widget.id === id ? { ...widget, isVisible: !widget.isVisible } : widget) })), [dispatch, widgets]);

  const renderWidget = (id: string) => {
    switch (id) {
      case 'recentlyAdded': return orderedMovies.slice(0, 5).map((movie) => <div key={movie.id}>{movie.title} <span>{movie.year}</span></div>);
      case 'upcomingReleases': case 'calendarPreview': return upcoming.slice(0, 5).map((movie) => <div key={movie.id}>{movie.title} <span>{new Date(movie.releaseDate!).toLocaleDateString()}</span></div>);
      case 'missingMonitored': return <strong>{missing.length}</strong>;
      case 'activeDownloads': return <strong>{queue?.length ?? 0}</strong>;
      case 'attention': return <><strong>{attention.length}</strong>{health.length ? <div>{health.length} system health message(s)</div> : null}</>;
      case 'libraryStatistics': return <div className={styles.stats}><span><strong>{movies.length}</strong> Movies</span><span><strong>{movies.filter((movie) => movie.hasFile).length}</strong> Downloaded</span><span><strong>{movies.filter((movie) => movie.monitored).length}</strong> Monitored</span></div>;
      case 'diskSpace': return diskSpace.length ? diskSpace.map((disk) => <div key={disk.path}>{disk.label ?? disk.path}: <strong>{formatBytes(disk.freeSpace)}</strong> free</div>) : <div>Disk information is loading…</div>;
      default: return null;
    }
  };

  return <PageContent title="Dashboard">
    <PageContentBody>
      <div className={styles.toolbar}><div><h1>Dashboard</h1><p>Your library at a glance.</p></div><button type="button" onClick={() => dispatch(setDashboardEditing({ isEditing: !isEditing }))}>{isEditing ? 'Done editing' : 'Edit layout'}</button></div>
      {isEditing ? <div className={styles.visibility} aria-label="Dashboard widget visibility">{widgets.map((widget) => <label key={widget.id}><input type="checkbox" checked={widget.isVisible} onChange={() => toggle(widget.id)} /> {widgetTitles[widget.id]}</label>)}</div> : null}
      <div className={styles.grid}>
        {widgets.filter((widget) => widget.isVisible).map((widget) => <section key={widget.id} className={styles.widget} draggable={isEditing} onDragStart={() => setDraggedId(widget.id)} onDragOver={(event) => { if (isEditing) event.preventDefault(); }} onDrop={() => { if (draggedId) moveBefore(draggedId, widget.id); setDraggedId(null); }} aria-labelledby={`widget-${widget.id}`}>
          <header><h2 id={`widget-${widget.id}`}>{widgetTitles[widget.id]}</h2>{isEditing ? <div className={styles.widgetControls}><button type="button" aria-label={`Move ${widgetTitles[widget.id]} earlier`} onClick={() => move(widget.id, -1)}>←</button><button type="button" aria-label={`Move ${widgetTitles[widget.id]} later`} onClick={() => move(widget.id, 1)}>→</button><button type="button" onClick={() => toggle(widget.id)}>Hide</button></div> : null}</header>
          <div className={styles.widgetBody}>{renderWidget(widget.id)}</div>
        </section>)}
      </div>
    </PageContentBody>
  </PageContent>;
}

export default DashboardPage;
