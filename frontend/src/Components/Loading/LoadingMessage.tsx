import React from 'react';
import styles from './LoadingMessage.css';

const messages = [
  'Downloading more RAM',
  'Now in Technicolor',
  'Previously on Radarr...',
  'Bleep Bloop.',
  'Locating the required gigapixels to render...',
  'Spinning up the hamster wheel...',
  "At least you're not on hold",
  'Hum something loud while others stare',
  'Loading humorous message... Please Wait',
  "I could've been faster in Python",
  "Don't forget to rewind your movies",
  'Congratulations! You are the 1000th visitor.',
  "HELP! I'm being held hostage and forced to write these stupid lines!",
  'RE-calibrating the internet...',
  "I'll be here all week",
  "Don't forget to tip your waitress",
  'Apply directly to the forehead',
  'Loading Battlestation',
  'Please wait while the minions do their work…',
  'Please wait… we are testing your patience.',
  'Loading… because why not?',
  'Rolling out the red carpet…',
  'Popcorn ready? Here we go!',
  'Editing the final cut…',
];

let message: string | null = null;

function LoadingMessage() {
  if (!message) {
    const index = Math.floor(Math.random() * messages.length);
    message = messages[index];
  }

  return <div className={styles.loadingMessage}>{message}</div>;
}

export default LoadingMessage;
