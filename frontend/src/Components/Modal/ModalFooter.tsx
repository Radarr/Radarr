import React from 'react';
import styles from './ModalFooter.css';

interface ModalFooterProps extends React.HTMLAttributes<HTMLDivElement> {
  children?: React.ReactNode;
}

function ModalFooter({ children, ...otherProps }: Readonly<ModalFooterProps>) {
  return (
    <div className={styles.modalFooter} {...otherProps}>
      {children}
    </div>
  );
}

export default ModalFooter;
