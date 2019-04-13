import React from 'react';
import styles from './LoadingMessage.css';

const messages = [
  'Welcome to Radarr Aphrodite Preview. Enjoy'
  // TODO Add some messages here
];

let message = null;

function LoadingMessage() {
  if (!message) {
    const index = Math.floor(Math.random() * messages.length);
    message = messages[index];
  }

  return (
    <div className={styles.loadingMessage}>
      {message}
    </div>
  );
}

export default LoadingMessage;
