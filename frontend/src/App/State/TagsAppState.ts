import ModelBase from 'App/ModelBase';
import AppSectionState, {
  AppSectionDeleteState,
  AppSectionSaveState,
} from 'App/State/AppSectionState';

export interface Tag extends ModelBase {
  label: string;
}

export interface TagDetail extends ModelBase {
  label: string;
  autoTagIds: number[];
  delayProfileIds: number[];
  downloadClientIds: number[];
  importListIds: number[];
  indexerIds: number[];
  movieIds: number[];
  notificationIds: number[];
  releaseProfileIds: number[];
}

export interface TagDetailAppState
  extends AppSectionState<TagDetail>,
    AppSectionDeleteState,
    AppSectionSaveState {}

interface TagsAppState extends AppSectionState<Tag>, AppSectionDeleteState {
  details: TagDetailAppState;
}

export default TagsAppState;
