import * as dark from './dark';
import * as light from './light';
import * as oled from './oled';

const defaultDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
const auto = defaultDark ? dark : light;

export default {
  auto,
  light,
  dark,
  oled
};
