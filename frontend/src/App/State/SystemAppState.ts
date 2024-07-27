import DiskSpace from 'typings/DiskSpace';
import Health from 'typings/Health';
import SystemStatus from 'typings/SystemStatus';
import Task from 'typings/Task';
import AppSectionState, { AppSectionItemState } from './AppSectionState';

export type DiskSpaceAppState = AppSectionState<DiskSpace>;
export type HealthAppState = AppSectionState<Health>;
export type SystemStatusAppState = AppSectionItemState<SystemStatus>;
export type TaskAppState = AppSectionState<Task>;

interface SystemAppState {
  diskSpace: DiskSpaceAppState;
  health: HealthAppState;
  status: SystemStatusAppState;
  tasks: TaskAppState;
}

export default SystemAppState;
