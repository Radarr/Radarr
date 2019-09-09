import { createSelector } from 'reselect';

function createMetadataProfileSelector() {
  return createSelector(
    (state, { metadataProfileId }) => metadataProfileId,
    (state) => state.settings.metadataProfiles.items,
    (metadataProfileId, metadataProfiles) => {
      return metadataProfiles.find((profile) => {
        return profile.id === metadataProfileId;
      });
    }
  );
}

export default createMetadataProfileSelector;
