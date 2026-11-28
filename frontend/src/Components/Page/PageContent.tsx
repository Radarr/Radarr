import React from 'react';
import DocumentTitle from 'react-document-title';
import ErrorBoundary from 'Components/Error/ErrorBoundary';
import PageContentError from './PageContentError';
import styles from './PageContent.css';

export const MAIN_CONTENT_ID = 'main-content';

interface PageContentProps {
  className?: string;
  title?: string;
  children: React.ReactNode;
}

function PageContent({
  className = styles.content,
  title,
  children,
}: PageContentProps) {
  return (
    <DocumentTitle
      title={
        title
          ? `${title} - ${window.Radarr.instanceName}`
          : window.Radarr.instanceName
      }
    >
      <main
        id={MAIN_CONTENT_ID}
        className={className}
        aria-label={title}
        tabIndex={-1}
      >
        <ErrorBoundary errorComponent={PageContentError}>
          {children}
        </ErrorBoundary>
      </main>
    </DocumentTitle>
  );
}

export default PageContent;
