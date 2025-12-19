import React from 'react';
import styles from './ModalHeader.css';

interface ModalHeaderProps extends React.HTMLAttributes<HTMLDivElement> {
  children?: React.ReactNode;
}

function ModalHeader({ children, ...otherProps }: Readonly<ModalHeaderProps>) {
  return (
    <div className={styles.modalHeader} {...otherProps}>
      {children}
    </div>
  );
}

export default ModalHeader;
