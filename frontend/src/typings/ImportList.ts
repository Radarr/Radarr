import { MovieMonitor } from 'Movie/Movie';
import Provider from './Provider';

interface ImportList extends Provider {
  enable: boolean;
  enabled: boolean;
  enableAuto: boolean;
  qualityProfileId: number;
  minimumAvailability: string;
  rootFolderPath: string;
  monitor: MovieMonitor;
  searchOnAdd: boolean;
  listType: string;
  listOrder: number;
  minRefreshInterval: string;
  name: string;
  tags: number[];
}

export default ImportList;
