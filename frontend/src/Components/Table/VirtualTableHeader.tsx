import React from 'react';
import styles from './VirtualTableHeader.css';

interface VirtualTableHeaderProps {
  children?: React.ReactNode;
}

function VirtualTableHeader({ children }: Readonly<VirtualTableHeaderProps>) {
  return <div className={styles.header}>{children}</div>;
}

export default VirtualTableHeader;
