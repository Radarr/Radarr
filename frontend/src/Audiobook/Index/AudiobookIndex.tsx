import React from 'react';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import translate from 'Utilities/String/translate';

function AudiobookIndex() {
  return (
    <PageContent title={translate('Audiobooks')}>
      <PageContentBody>
        <div style={{ padding: '20px', textAlign: 'center' }}>
          <h1>{translate('Audiobooks')}</h1>
          <p>Audiobook management coming soon.</p>
          <p>This feature is part of Phase 3 development.</p>
        </div>
      </PageContentBody>
    </PageContent>
  );
}

export default AudiobookIndex;
