export type PersonalizedTheme = 'system' | 'light' | 'dark' | 'oled';
export type InterfaceDensity = 'compact' | 'comfortable' | 'spacious';
export type PosterSize = 'small' | 'medium' | 'large';
export type CardStyle = 'rounded' | 'square';

export interface DashboardWidgetPreference {
  id: string;
  isVisible: boolean;
}

interface PersonalizedUiAppState {
  theme: PersonalizedTheme;
  accentColor: string;
  density: InterfaceDensity;
  posterSize: PosterSize;
  cardStyle: CardStyle;
  enableAnimations: boolean;
  enableBackdrops: boolean;
  isSidebarCollapsed: boolean;
  dashboard: {
    isEditing: boolean;
    widgets: DashboardWidgetPreference[];
  };
}

export default PersonalizedUiAppState;
