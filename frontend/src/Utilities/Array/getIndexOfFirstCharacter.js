export default function getIndexOfFirstCharacter(items, character) {
  return items.findIndex((item) => {
    const firstCharacter = item.sortTitle.charAt(0);

    if (character === '#') {
      return !Number.Number.isNaN(firstCharacter);
    }

    return firstCharacter === character;
  });
}
