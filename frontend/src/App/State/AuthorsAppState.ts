import AppSectionState, {
  AppSectionDeleteState,
  AppSectionSaveState,
} from 'App/State/AppSectionState';
import Author from 'Author/Author';

interface AuthorsAppState
  extends AppSectionState<Author>, AppSectionDeleteState, AppSectionSaveState {
  pendingChanges: Partial<Author>;
}

export default AuthorsAppState;
