import AppSectionState from 'App/State/AppSectionState';
import Audiobook from 'Audiobook/Audiobook';

interface AddAudiobookDefaults {
  rootFolderPath: string;
  monitor: boolean;
  qualityProfileId: number;
  searchForAudiobook: boolean;
  tags: number[];
}

interface AddAudiobookAppState extends AppSectionState<Audiobook> {
  isAdding: boolean;
  isAdded: boolean;
  addError: Error | null;
  defaults: AddAudiobookDefaults;
}

export default AddAudiobookAppState;
