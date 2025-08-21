import React, { useMemo } from 'react';
import { useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import { Message as MessageModel } from 'App/State/MessagesAppState';
import Message from './Message';
import styles from './Messages.css';

interface GroupedMessage extends MessageModel {
  count: number;
  originalIds: number[];
}

function createMessageGroupKey(message: MessageModel): string {
  return `${message.message}|${message.type}|${message.name}`;
}

function groupMessages(messages: MessageModel[]): GroupedMessage[] {
  const messageMap = new Map<string, GroupedMessage>();

  messages.forEach((message) => {
    const key = createMessageGroupKey(message);
    const existingGroup = messageMap.get(key);

    if (existingGroup) {
      existingGroup.count += 1;
      existingGroup.originalIds.push(message.id);
      if (message.hideAfter > 0) {
        existingGroup.hideAfter = message.hideAfter;
      }
    } else {
      messageMap.set(key, {
        ...message,
        count: 1,
        originalIds: [message.id],
      });
    }
  });

  return Array.from(messageMap.values());
}

function sortGroupedMessages(
  groupedMessages: GroupedMessage[]
): GroupedMessage[] {
  return groupedMessages.sort((a, b) => {
    const aIsGrouped = a.count > 1;
    const bIsGrouped = b.count > 1;

    if (aIsGrouped && !bIsGrouped) {
      return 1;
    }
    if (!aIsGrouped && bIsGrouped) {
      return -1;
    }

    if (!aIsGrouped && !bIsGrouped) {
      const maxIdA = Math.max(...a.originalIds);
      const maxIdB = Math.max(...b.originalIds);
      return maxIdB - maxIdA;
    }

    return a.count - b.count;
  });
}

function Messages() {
  const messages = useSelector((state: AppState) => state.app.messages.items);

  const sortedGroupedMessages = useMemo(() => {
    if (!messages.length) {
      return [];
    }

    const grouped = groupMessages(messages);
    return sortGroupedMessages(grouped);
  }, [messages]);

  return (
    <div className={styles.messages}>
      {sortedGroupedMessages.map((message) => {
        const messageKey = `grouped-${message.originalIds.join('-')}`;
        const mostRecentId = Math.max(...message.originalIds);

        return (
          <Message
            key={messageKey}
            {...message}
            id={mostRecentId}
            originalIds={message.originalIds}
          />
        );
      })}
    </div>
  );
}

export default Messages;
