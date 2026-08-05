import * as dark from './dark';
import * as light from './light';
import * as oled from './oled';

// ApplyTheme resolves Auto at runtime so it can react to system-theme changes.
const auto = light;

export default {
  auto,
  light,
  dark,
  oled
};
