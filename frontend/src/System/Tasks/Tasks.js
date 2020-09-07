import React from 'react';
import PageContent from 'Components/Page/PageContent';
import PageContentBodyConnector from 'Components/Page/PageContentBodyConnector';
import QueuedTasksConnector from './Queued/QueuedTasksConnector';
import ScheduledTasksConnector from './Scheduled/ScheduledTasksConnector';

function Tasks() {
  return (
    <PageContent title="Tasks">
      <PageContentBodyConnector>
        <ScheduledTasksConnector />
        <QueuedTasksConnector />
      </PageContentBodyConnector>
    </PageContent>
  );
}

export default Tasks;
