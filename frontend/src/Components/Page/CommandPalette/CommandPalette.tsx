import React, { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { useSelector } from 'react-redux';
import { useHistory } from 'react-router-dom';
import AppState from 'App/State/AppState';
import Modal from 'Components/Modal/Modal';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import Movie from 'Movie/Movie';
import getPathWithUrlBase from 'Utilities/getPathWithUrlBase';
import styles from './CommandPalette.css';

const commands = [
  { label: 'Dashboard', path: '/dashboard' },
  { label: 'Movies', path: '/' },
  { label: 'Add movie', path: '/add/new' },
  { label: 'Calendar', path: '/calendar' },
  { label: 'Activity', path: '/activity/queue' },
  { label: 'Settings', path: '/settings' },
  { label: 'System status', path: '/system/status' },
];

function CommandPalette() {
  const history = useHistory();
  const inputRef = useRef<HTMLInputElement>(null);
  const [isOpen, setIsOpen] = useState(false);
  const [query, setQuery] = useState('');
  const movies = useSelector((state: AppState) => state.movies.items) as Movie[];

  const close = useCallback(() => { setIsOpen(false); setQuery(''); }, []);
  const openPath = useCallback((path: string) => { history.push(getPathWithUrlBase(path)); close(); }, [close, history]);

  useEffect(() => {
    const onKeyDown = (event: KeyboardEvent) => {
      const target = event.target as HTMLElement | null;
      const isTyping = target?.matches('input, textarea, select, [contenteditable="true"]');
      if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === 'k' && !isTyping) {
        event.preventDefault();
        setIsOpen(true);
      }
    };
    window.addEventListener('keydown', onKeyDown);
    return () => window.removeEventListener('keydown', onKeyDown);
  }, []);

  useEffect(() => { if (isOpen) window.setTimeout(() => inputRef.current?.focus(), 0); }, [isOpen]);

  const results = useMemo(() => {
    const needle = query.trim().toLowerCase();
    const navigation = commands.filter((item) => !needle || item.label.toLowerCase().includes(needle));
    const movieResults = !needle ? [] : movies.filter((movie) => movie.title.toLowerCase().includes(needle)).slice(0, 8).map((movie) => ({ label: `${movie.title} (${movie.year})`, path: `/movie/${movie.titleSlug}` }));
    return [...navigation, ...movieResults];
  }, [movies, query]);

  return (
    <Modal isOpen={isOpen} size="medium" onModalClose={close}>
      <ModalContent className={styles.content} onModalClose={close} aria-label="Command palette">
        <input
          ref={inputRef}
          aria-label="Command palette search"
          className={styles.input}
          placeholder="Navigate or search movies…"
          value={query}
          onChange={(event) => setQuery(event.target.value)}
          onKeyDown={(event) => {
            if (event.key === 'Enter' && results[0]) {
              openPath(results[0].path);
            }
          }}
        />
        <ModalBody className={styles.results}>
          {results.map((item) => <button className={styles.result} type="button" key={`${item.path}-${item.label}`} onClick={() => openPath(item.path)}>{item.label}</button>)}
          {!results.length ? <div className={styles.empty}>No matching commands or movies</div> : null}
        </ModalBody>
        <div className={styles.hint}>Enter to select · Esc to close · Ctrl/Cmd + K to open</div>
      </ModalContent>
    </Modal>
  );
}

export default CommandPalette;
