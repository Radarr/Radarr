import React from 'react';

interface TableBodyProps {
  children?: React.ReactNode;
}

function TableBody({ children }: Readonly<TableBodyProps>) {
  return <tbody>{children}</tbody>;
}

export default TableBody;
