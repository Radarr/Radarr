import classNames from 'classnames';
import React, { useCallback, useEffect, useMemo, useRef } from 'react';
import { useDispatch } from 'react-redux';
import { MessageType } from 'App/State/MessagesAppState';
import Icon, { IconName } from 'Components/Icon';
import { icons } from 'Helpers/Props';
import { hideMessage } from 'Store/Actions/appActions';
import styles from './Message.css';

interface MessageProps {
  id: number;
  hideAfter: number;
  name: string;
  message: string;
  type: Extract<MessageType, keyof typeof styles>;
  count?: number;
  originalIds?: number[];
}

function getMessageIcon(name: string): IconName {
  switch (name) {
    case 'ApplicationUpdate':
      return icons.RESTART;
    case 'Backup':
      return icons.BACKUP;
    case 'CheckHealth':
      return icons.HEALTH;
    case 'Housekeeping':
      return icons.HOUSEKEEPING;
    case 'MoviesSearch':
      return icons.SEARCH;
    case 'RefreshMovie':
      return icons.REFRESH;
    case 'RssSync':
      return icons.RSS;
    default:
      return icons.SPINNER;
  }
}

function Message({
  id,
  hideAfter,
  name,
  message,
  type,
  count = 1,
  originalIds = [id],
}: MessageProps) {
  const dispatch = useDispatch();
  const dismissTimeout = useRef<ReturnType<typeof setTimeout>>();

  const icon = useMemo(() => getMessageIcon(name), [name]);

  const hideAllGroupedMessages = useCallback(() => {
    originalIds.forEach((messageId) => {
      dispatch(hideMessage({ id: messageId }));
    });
  }, [originalIds, dispatch]);

  useEffect(() => {
    if (!hideAfter) {
      return;
    }

    dismissTimeout.current = setTimeout(() => {
      hideAllGroupedMessages();
      dismissTimeout.current = undefined;
    }, hideAfter * 1000);

    return () => {
      if (dismissTimeout.current) {
        clearTimeout(dismissTimeout.current);
      }
    };
  }, [hideAfter, hideAllGroupedMessages]);

  return (
    <div className={classNames(styles.message, styles[type])}>
      <div className={styles.iconContainer}>
        <Icon name={icon} title={name} />
      </div>

      <div className={styles.text}>{message}</div>

      {count > 1 && <div className={styles.countBadge}>{count}</div>}
    </div>
  );
}

export default Message;
