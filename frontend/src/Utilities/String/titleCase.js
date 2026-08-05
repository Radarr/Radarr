const regex = /\b\w+/g;

function titleCase(input) {
  if (!input) {
    return '';
  }

  return input.replace(regex, (match) => {
    if (match.toLowerCase() === 'oled') {
      return 'OLED';
    }

    return match.charAt(0).toUpperCase() + match.substr(1).toLowerCase();
  });
}

export default titleCase;
