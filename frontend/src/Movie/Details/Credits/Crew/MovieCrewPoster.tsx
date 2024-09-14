import classNames from 'classnames';
import React, { useCallback, useState } from 'react';
import { useDispatch } from 'react-redux';
import Icon from 'Components/Icon';
import Label from 'Components/Label';
import Link from 'Components/Link/Link';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import MonitorToggleButton from 'Components/MonitorToggleButton';
import Popover from 'Components/Tooltip/Popover';
import useModalOpenState from 'Helpers/Hooks/useModalOpenState';
import { icons, kinds, sizes } from 'Helpers/Props';
import MovieHeadshot from 'Movie/MovieHeadshot';
import EditImportListModalConnector from 'Settings/ImportLists/ImportLists/EditImportListModalConnector';
import { deleteImportList } from 'Store/Actions/Settings/importLists';
import ImportList from 'typings/ImportList';
import MovieCredit from 'typings/MovieCredit';
import translate from 'Utilities/String/translate';
import styles from '../MovieCreditPoster.css';

export interface MovieCrewPosterProps
  extends Pick<MovieCredit, 'personName' | 'images' | 'job'> {
  tmdbId: number;
  posterWidth: number;
  posterHeight: number;
  importList?: ImportList;
  onImportListSelect(): void;
}

function MovieCrewPoster(props: MovieCrewPosterProps) {
  const {
    tmdbId,
    personName,
    job,
    images = [],
    posterWidth,
    posterHeight,
    importList,
    onImportListSelect,
  } = props;

  const importListId = importList?.id ?? 0;

  const dispatch = useDispatch();

  const [hasPosterError, setHasPosterError] = useState(false);

  const [
    isEditImportListModalOpen,
    setEditImportListModalOpen,
    setEditImportListModalClosed,
  ] = useModalOpenState(false);

  const [
    isDeleteImportListModalOpen,
    setDeleteImportListModalOpen,
    setDeleteImportListModalClosed,
  ] = useModalOpenState(false);

  const handlePosterLoadError = useCallback(() => {
    setHasPosterError(true);
  }, [setHasPosterError]);

  const handlePosterLoad = useCallback(() => {
    setHasPosterError(false);
  }, [setHasPosterError]);

  const handleManageImportListPress = useCallback(() => {
    if (importListId === 0) {
      onImportListSelect();
    }

    setEditImportListModalOpen();
  }, [importListId, onImportListSelect, setEditImportListModalOpen]);

  const handleDeleteImportListConfirmed = useCallback(() => {
    dispatch(deleteImportList({ id: importListId }));

    setEditImportListModalClosed();
    setDeleteImportListModalClosed();
  }, [
    importListId,
    setEditImportListModalClosed,
    setDeleteImportListModalClosed,
    dispatch,
  ]);

  const elementStyle = {
    width: `${posterWidth}px`,
    height: `${posterHeight}px`,
    borderRadius: '5px',
  };

  const contentStyle = {
    width: `${posterWidth}px`,
  };

  const monitored =
    importList?.enabled === true && importList?.enableAuto === true;

  return (
    <div className={styles.content} style={contentStyle}>
      <div className={styles.posterContainer}>
        <div className={styles.toggleMonitoredContainer}>
          <MonitorToggleButton
            className={styles.monitorToggleButton}
            monitored={monitored}
            size={20}
            onPress={handleManageImportListPress}
          />
        </div>

        <Label className={styles.controls}>
          <span className={styles.externalLinks}>
            <Popover
              anchor={<Icon name={icons.EXTERNAL_LINK} size={12} />}
              title={translate('Links')}
              body={
                <Link to={`https://www.themoviedb.org/person/${tmdbId}`}>
                  <Label
                    className={styles.externalLinkLabel}
                    kind={kinds.INFO}
                    size={sizes.LARGE}
                  >
                    {translate('TMDb')}
                  </Label>
                </Link>
              }
            />
          </span>
        </Label>

        <div style={elementStyle}>
          <MovieHeadshot
            className={styles.poster}
            style={elementStyle}
            images={images}
            size={250}
            lazy={false}
            overflow={true}
            onError={handlePosterLoadError}
            onLoad={handlePosterLoad}
          />

          {hasPosterError && (
            <div className={styles.overlayTitle}>{personName}</div>
          )}
        </div>
      </div>

      <div className={classNames(styles.title, 'swiper-no-swiping')}>
        {personName}
      </div>
      <div className={classNames(styles.title, 'swiper-no-swiping')}>{job}</div>

      <EditImportListModalConnector
        id={importListId}
        isOpen={isEditImportListModalOpen}
        onModalClose={setEditImportListModalClosed}
        onDeleteImportListPress={setDeleteImportListModalOpen}
      />

      <ConfirmModal
        isOpen={isDeleteImportListModalOpen}
        kind={kinds.DANGER}
        title={translate('DeleteImportList')}
        message={translate('DeleteImportListMessageText', {
          name: importList?.name ?? personName,
        })}
        confirmLabel={translate('Delete')}
        onConfirm={handleDeleteImportListConfirmed}
        onCancel={setDeleteImportListModalClosed}
      />
    </div>
  );
}

export default MovieCrewPoster;
