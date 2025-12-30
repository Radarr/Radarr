import AppSectionState, {
  AppSectionDeleteState,
  AppSectionSaveState,
} from 'App/State/AppSectionState';
import BookSeries from 'BookSeries/BookSeries';

interface BookSeriesAppState
  extends
    AppSectionState<BookSeries>,
    AppSectionDeleteState,
    AppSectionSaveState {
  pendingChanges: Partial<BookSeries>;
}

export default BookSeriesAppState;
