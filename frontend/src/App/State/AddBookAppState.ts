import AppSectionState from 'App/State/AppSectionState';
import Book from 'Book/Book';

interface AddBookDefaults {
  rootFolderPath: string;
  monitor: boolean;
  qualityProfileId: number;
  searchForBook: boolean;
  tags: number[];
}

interface AddBookAppState extends AppSectionState<Book> {
  isAdding: boolean;
  isAdded: boolean;
  addError: Error | null;
  defaults: AddBookDefaults;
}

export default AddBookAppState;
