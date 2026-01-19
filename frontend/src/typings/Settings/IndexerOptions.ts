export default interface IndexerOptions {
  minimumAge: number;
  retention: number;
  maximumSize: number;
  rssSyncInterval: number;
  preferIndexerFlags: boolean;
  seedersPreference: string;
  availabilityDelay: number;
  whitelistedHardcodedSubs: string[];
  allowHardcodedSubs: boolean;
}
