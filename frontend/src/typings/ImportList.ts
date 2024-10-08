import ModelBase from 'App/ModelBase';

export interface Field {
  order: number;
  name: string;
  label: string;
  value: boolean | number | string;
  type: string;
  advanced: boolean;
  privacy: string;
}

interface ImportList extends ModelBase {
  enable: boolean;
  enabled: boolean;
  enableAuto: boolean;
  qualityProfileId: number;
  minimumAvailability: string;
  rootFolderPath: string;
  name: string;
  fields: Field[];
  implementationName: string;
  implementation: string;
  configContract: string;
  infoLink: string;
  tags: number[];
  retroApplyTags: boolean;
}

export default ImportList;
