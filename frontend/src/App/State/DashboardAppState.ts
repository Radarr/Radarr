import { AppSectionItemState } from './AppSectionState';

export interface MediaTypeStatistics {
  total: number;
  withFiles: number;
  missing: number;
  monitored: number;
  unmonitored: number;
  sizeOnDisk: number;
  totalDurationMinutes: number;
}

export interface DashboardStatistics {
  movies: MediaTypeStatistics;
  books: MediaTypeStatistics;
  audiobooks: MediaTypeStatistics;
  totalSizeOnDisk: number;
}

type DashboardAppState = AppSectionItemState<DashboardStatistics>;

export default DashboardAppState;
