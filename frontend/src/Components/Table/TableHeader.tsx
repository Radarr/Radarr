import React from 'react';

interface TableHeaderProps {
  children?: React.ReactNode;
}

function TableHeader({ children }: Readonly<TableHeaderProps>) {
  return (
    <thead>
      <tr>{children}</tr>
    </thead>
  );
}

export default TableHeader;
