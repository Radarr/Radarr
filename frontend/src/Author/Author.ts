import ModelBase from 'App/ModelBase';

interface Author extends ModelBase {
  name: string;
  sortName: string;
  description: string;
  foreignAuthorId: string;
  monitored: boolean;
  qualityProfileId: number;
  path: string;
  rootFolderPath: string;
  added: string;
  tags: number[];
  isSaving?: boolean;
}

export default Author;
