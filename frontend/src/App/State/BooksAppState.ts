import AppSectionState, {
  AppSectionDeleteState,
  AppSectionSaveState,
} from 'App/State/AppSectionState';
import Book from 'Book/Book';

interface BooksAppState
  extends AppSectionState<Book>, AppSectionDeleteState, AppSectionSaveState {
  pendingChanges: Partial<Book>;
}

export default BooksAppState;
