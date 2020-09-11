function firstCharToUpper(input) {
  if (!input) {
    return '';
  }

  return [].map.call(input, (char, i) => (i ? char : char.toUpperCase())).join('');
}

export default firstCharToUpper;
